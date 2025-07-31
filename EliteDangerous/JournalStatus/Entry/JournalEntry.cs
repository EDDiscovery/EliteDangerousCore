/*
 * Copyright 2016-2022 EDDiscovery development team
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
using QuickJSON;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace EliteDangerousCore
{
    [System.Diagnostics.DebuggerDisplay("Event {EventTypeStr} {EventTimeUTC} JID {Id} C {CommanderId}")]
    public abstract partial class JournalEntry
    {
        #region Public Instance properties and fields

        public long Id { get; private set; }                    // this is the entry ID
        public long TLUId { get; private set; }                 // this ID of the journal tlu (aka TravelLogId)
        public bool IsJournalSourced { get { return TLUId > 0; } }      // generated entries by EDD will have a TLU of 0 - others, file and CAPI journal downloaded, will not
        public int CommanderId { get; private set; }            // commander Id of entry

        public JournalTypeEnum EventTypeID { get; private set; }
        [QuickJSON.JsonIgnore()]
        public string EventTypeStr { get { return EventTypeID.ToString(); } }             // name of event. these two duplicate each other, string if for debuggin in the db view of a browser

        public System.Drawing.Image Icon() { return JournalTypeIcons.ContainsKey(this.IconEventType) ? JournalTypeIcons[this.IconEventType] : JournalTypeIcons[JournalTypeEnum.Unknown]; }    // Icon to paint for this
        public string GetIconPackPath() { return "Journal." + IconEventType.ToString(); } // its icon pack name..

        public DateTime EventTimeUTC { get; set; }

        public DateTime EventTimeLocal { get { return EventTimeUTC.ToLocalTime(); } }

        [QuickJSON.JsonIgnore()]
        public bool SyncedEDSM { get { return (Synced & (int)SyncFlags.EDSM) != 0; } }
        [QuickJSON.JsonIgnore()]
        public bool SyncedEDDN { get { return (Synced & (int)SyncFlags.EDDN) != 0; } }
        [QuickJSON.JsonIgnore()]
        public bool StartMarker { get { return (Synced & (int)SyncFlags.StartMarker) != 0; } }
        [QuickJSON.JsonIgnore()]
        public bool StopMarker { get { return (Synced & (int)SyncFlags.StopMarker) != 0; } }

        public virtual bool IsBeta { get { return TravelLogUnit.Get(TLUId)?.IsBeta ?? DefaultBetaFlag; } }        // TLUs are cached via the dictionary, no point also holding a local copy
        public virtual bool IsHorizons { get { return TravelLogUnit.Get(TLUId)?.IsHorizons ?? DefaultHorizonsFlag; } }  // horizons flag from loadgame
        public virtual bool IsOdyssey { get { return TravelLogUnit.Get(TLUId)?.IsOdyssey ?? DefaultOdysseyFlag; } } // odyseey flag from loadgame

        public bool IsOdysseyEstimatedValues() 
        {
            var tlu = TravelLogUnit.Get(TLUId);
            if (tlu != null)
                return IsOdyssey || GameVersion.StartsWith("4.");       // if odyssey flag is set, OR we have a horizons 4.0 situation
            else
                return DefaultOdysseyFlag;
        }

        public virtual string GameVersion { get { return TravelLogUnit.Get(TLUId)?.GameVersion ?? ""; } }
        public virtual string Build { get { return TravelLogUnit.Get(TLUId)?.Build ?? ""; } }
        public virtual string FullPath { get { return TravelLogUnit.Get(TLUId)?.FullName ?? ""; } }

        public static bool DefaultBetaFlag { get; set; } = false;
        public static bool DefaultHorizonsFlag { get; set; } = false;       // for entries without a TLU (EDSM downloaded made up ones for instance) provide default value
        public static bool DefaultOdysseyFlag { get; set; } = false;

        public SystemNoteClass SNC { get; set; }                            // if journal entry has a system note attached. Null if not

        public class FillInformationData
        {
            public ISystem System { get; set; }
            public string WhereAmI { get; set; }
            public string BodyName { get; set; }
            public string NextJumpSystemName { get; set; }
            public long? NextJumpSystemAddress { get; set; }
        };

        // these may or may not be overriden by class.  

        public virtual string GetInfo()         // if overridden, always return a string
        {
            return null;
        }
        public virtual string GetInfo(FillInformationData fid)  // if overridden, always return a string
        {
            return null;
        }
        public virtual string GetDetailed() // may return null if no more data available
        {
            return null;
        }
        public virtual string GetDetailed(FillInformationData fid) // may return null if no more data available
        {
            return null;
        }

        // the long name of it, such as Approach Body. May be overridden, is translated
        public virtual string SummaryName(ISystem sys) { return TranslatedEventNames.ContainsKey(EventTypeID) ? TranslatedEventNames[EventTypeID] : EventTypeID.ToString(); }  // entry may be overridden for specialist output

        // the name used to filter it.. and the filter keyword. Its normally the enum of the event.
        [QuickJSON.JsonIgnore()]
        public virtual string EventFilterName { get { return EventTypeID.ToString(); } } // text name used in filter

        #endregion

        #region Special Setters - db not updated by them

        public void SetTLUCommander(long t, int cmdr)         // used during log reading..
        {
            TLUId = t;
            CommanderId = cmdr;
        }

        public void SetCommander(int cmdr)         // used during creation of special jrec
        {
            CommanderId = cmdr;
        }

        public void SetJID(long id)         // used if substituting one record for another
        {
            Id = id;
        }

        public void SetSystemNote()         // see if a system note can be assigned.
        {
            SNC = SystemNoteClass.GetJIDNote(Id);
            if (SNC == null && EventTypeID == JournalTypeEnum.FSDJump)      // if null and FSD Jump
            {
                string system = ((JournalFSDJump)this).StarSystem;          // try the star system
                SNC = SystemNoteClass.GetSystemNote(system);
            }

            //if (SNC != null) System.Diagnostics.Debug.WriteLine($"Journal System note found for {Id}");
        }

        // update the note. If text is blank, delete it
        public void UpdateSystemNote(string text, string system, bool sendtoedsm)
        {
            System.Diagnostics.Trace.Assert(text != null && system != null);

            bool fsdentry = EventTypeID == JournalTypeEnum.FSDJump;

            if (SNC == null)            // if no system note, we make one. Its a JID note unless its on a FSD jump, in which case its a system note. Syncs with EDSM in that case
                SNC = SystemNoteClass.MakeNote(text, DateTime.Now, system, fsdentry ? 0 : Id, GetJson().ToString());        
            else
                SNC = SNC.UpdateNote(text, DateTime.Now, GetJson().ToString());    // and update info, and update our ref in case it has changed or gone null

            if (SNC != null && sendtoedsm && fsdentry )    // if still have a note and send to esdm, and its on an FSD entry, then its a system note
                EDSM.EDSMClass.SendComments(SNC.SystemName, SNC.Note);
        }

        #endregion

        #region Setters - db is updated

        public void SetStartFlag()
        {
            UserDatabase.Instance.DBWrite(cn => UpdateSyncFlagBit(SyncFlags.StartMarker, true, SyncFlags.StopMarker, false, cn));
        }

        public void SetEndFlag()
        {
            UserDatabase.Instance.DBWrite(cn => UpdateSyncFlagBit(SyncFlags.StartMarker, false, SyncFlags.StopMarker, true, cn));
        }

        public void ClearStartEndFlag()
        {
            UserDatabase.Instance.DBWrite(cn => UpdateSyncFlagBit(SyncFlags.StartMarker, false, SyncFlags.StopMarker, false, cn));
        }

        public void SetEdsmSync(bool value = true)
        {
            UserDatabase.Instance.DBWrite(cn => UpdateSyncFlagBit(SyncFlags.EDSM, value, SyncFlags.NoBit, false, cn));
        }

        internal void SetEdsmSync(SQLiteConnectionUser cn , DbTransaction txn = null, bool value = true)
        {
            UpdateSyncFlagBit(SyncFlags.EDSM, value, SyncFlags.NoBit, false, cn, txn);
        }

        public void SetEddnSync(bool value = true)
        {
            UserDatabase.Instance.DBWrite( cn => UpdateSyncFlagBit(SyncFlags.EDDN, value , SyncFlags.NoBit, false, cn));
        }

        public static void SetEdsmSyncList(List<JournalEntry> jlist, bool value = true)
        {
            UserDatabase.Instance.DBWrite(cn =>
            {
                using (var txn = cn.BeginTransaction())
                {
                    foreach (var he in jlist)
                        he.SetEdsmSync(cn, txn, value);
                    txn.Commit();
                }
            });
        }


        #endregion

        #region Event Information - return event enums/icons/text etc.

        // return JEnums with events matching optional methods, unsorted
        static public List<JournalTypeEnum> GetEnumOfEvents(string[] methods = null)
        {
            List<JournalTypeEnum> ret = new List<JournalTypeEnum>();

            foreach (JournalTypeEnum jte in Enum.GetValues(typeof(JournalTypeEnum)))
            {
                if ((int)jte < (int)JournalTypeEnum.ObsoleteOrIcons)
                {
                    if (methods == null)
                    {
                        ret.Add(jte);
                    }
                    else
                    {
                        Type jtype = TypeOfJournalEntry(jte);

                        if (jtype != null)      // may be null, Unknown for instance
                        {
                            foreach (var n in methods)
                            {
                                if (jtype.GetMethod(n) != null)
                                {
                                    ret.Add(jte);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }

        // return name instead of enum, unsorted
        static public List<string> GetNameOfEvents(string[] methods = null)
        {
            var list = GetEnumOfEvents(methods);
            return list.Select(x => x.ToString()).ToList();
        }

        // enum name, translated name, image
        static public List<Tuple<string, string, Image>> GetNameImageOfEvents(string[] methods = null, bool sort = false)
        {
            List<JournalTypeEnum> jevents = JournalEntry.GetEnumOfEvents(methods);

            var list = jevents.Select(x => new Tuple<string, string, Image>(x.ToString(), TranslatedEventNames[x],
                JournalTypeIcons.ContainsKey(x) ? JournalTypeIcons[x] : JournalTypeIcons[JournalTypeEnum.Unknown])).ToList();

            if (sort)
            {
                list.Sort(delegate (Tuple<string, string, Image> left, Tuple<string, string, Image> right)     // in order, oldest first
                {
                    return left.Item2.ToString().CompareTo(right.Item2.ToString());
                });
            }

            return list;
        }

        static public Tuple<string, string, Image> GetNameImageOfEvent(JournalTypeEnum ev)
        {
            return new Tuple<string, string, Image>(ev.ToString(), TranslatedEventNames[ev],
                JournalTypeIcons.ContainsKey(ev) ? JournalTypeIcons[ev] : JournalTypeIcons[JournalTypeEnum.Unknown]);
        }

        #endregion

        #region Factory creation

        static public JournalEntry CreateJournalEntry(string events, DateTime t)            
        {
            JObject jo = new JObject();
            jo.Add("event", events);
            jo.Add("timestamp", t);
            return CreateJournalEntry(jo.ToString());
        }

        static protected JournalEntry CreateJournalEntry(long id, long tluid, int cmdrid, string json, int syncflags)      
        {
            JournalEntry jr = JournalEntry.CreateJournalEntry(json);
            jr.Id = id;
            jr.TLUId = tluid;
            jr.CommanderId = cmdrid;
            jr.Synced = syncflags;
            return jr;
        }

        // Decode text, to journal entry, or Unknown/Null if bad
        static public JournalEntry CreateJournalEntry(string text, bool savejson = false, bool returnnullifbadjson = false)       
        {
            JObject jo = JToken.Parse(text, JToken.ParseOptions.AllowTrailingCommas | JToken.ParseOptions.CheckEOL).Object();
            JournalEntry ret = null;

            if (jo != null)         // good json
            {
                string eventname = jo["event"].StrNull();

                if (eventname != null)  // has an event name, therefore worth keeping
                {
                    if (ClassActivators.TryGetValue(eventname, out var act))        // if known, make it
                        ret = act(jo);
                    else
                    {
                        ret = new JournalUnknown(jo);           // else make a unknown one
                        System.Diagnostics.Debug.WriteLine("Not Recognised event " + jo.ToString());
                    }
                }
            }

            if ( ret == null )                      // no journal line
            {
                if (returnnullifbadjson)            // if we just want to dump it, return null
                    return null;

                jo = new JObject();                 // otherwise, make a JSON for display purposes with BadJSON with the text in
                jo["BadJSON"] = text;               // used if we read bad JSON from the DB
                ret = new JournalUnknown(jo);       // unknown
                savejson = true;                    // need to keep this JSON as we made this up
                System.Diagnostics.Debug.WriteLine("Bad JSON" + text);
            }

            if (savejson)
                ret.JsonCached = jo;

            return ret;
        }

        private class JETable
        {
            public int processor;
            public List<TableData> table;
            public JournalEntry[] results;
            public int start;
            public int end;
            public CountdownEvent finished;
            public CancellationToken iscancelled;
            public Action<int,float> progress;
        }

        private class Stats
        {
            public JournalTypeEnum type;
            public int number;
            public long ticks;
        }

        // from table data ID, travellogid, commanderid, event json, sync flag
        // create journal entries.  Multithreaded if many entries
        // null if cancelled
        static public JournalEntry[] CreateJournalEntries(List<TableData> tabledata, CancellationToken cancel, Action<int> progress = null, bool multithread = true)
        {
            JournalEntry[] jlist = new JournalEntry[tabledata.Count];

            if (multithread && tabledata.Count > 10000 && System.Environment.ProcessorCount > 1)  // a good amount, worth MTing - just in case someone is using this on a potato
            {
                int threads = Math.Max(System.Environment.ProcessorCount * 3 / 4, 2);   // leave a few processors for other things

                CountdownEvent cd = new CountdownEvent(threads);
                float[] threadprogress = new float[threads];

                for (int i = 0; i < threads; i++)
                {
                    int s = i * tabledata.Count / threads;
                    int e = (i + 1) * tabledata.Count / threads;
                    Thread t1 = new Thread(new ParameterizedThreadStart(CreateJEinThread));
                    t1.Priority = ThreadPriority.Highest;
                    t1.Name = $"JournalEntry GetAll {i}";

                    System.Diagnostics.Trace.WriteLine($"{BaseUtils.AppTicks.TickCount} Journal Creation Spawn {s}..{e}");

                    var data = new JETable() { processor = i, table = tabledata, results = jlist, start = s, end = e, finished = cd,
                        iscancelled = cancel,
                        progress = (tno, p) => {
                            threadprogress[tno] = p;
                            int percent = (int)(100 * threadprogress.Sum() / (threads));
                            progress?.Invoke(percent);
                            // System.Diagnostics.Debug.WriteLine($"CJE progress {p} from {tno} of {threads}, total {percent}");
                        }
                    };
                    t1.Start(data);
                }

                cd.Wait();
            }
            else
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                Dictionary<JournalTypeEnum, Stats> ticks = new Dictionary<JournalTypeEnum, Stats>();

                // code to measure performance.. use this by turning off multitasking and either select individual or total time

#if true
                // measure each one

                for (int j = 0; j < tabledata.Count; j++)
                {
                    long ticks1 = sw.ElapsedTicks;
                    var e = tabledata[j];
                    jlist[j] = CreateJournalEntry(e.ID, e.TLUID, e.Cmdr, e.Json, e.Syncflag);
                    long ticks2 = sw.ElapsedTicks;
                    if (!ticks.TryGetValue(jlist[j].EventTypeID, out Stats ticksn))
                        ticks[jlist[j].EventTypeID] = new Stats() { ticks = ticks2 - ticks1, number = 1, type = jlist[j].EventTypeID };
                    else
                    {
                        ticks[jlist[j].EventTypeID].number++;
                        ticks[jlist[j].EventTypeID].ticks += ticks2 - ticks1;
                    }
                }
#else
                // measure total

                long ticks1 = sw.ElapsedTicks;

                for (int j = 0; j < tabledata.Count; j++)
                {
                    var e = tabledata[j];
                    jlist[j] = CreateJournalEntry(e.ID, e.TLUID, e.Cmdr, e.Json, e.Syncflag);
                }
                long ticks2 = sw.ElapsedTicks;

                ticks[JournalTypeEnum.Unknown] = new Stats() { ticks = ticks2 - ticks1, number = 1, type = JournalTypeEnum.Unknown };

#endif
                //{
                //    var values = ticks.Values.ToList();
                //    values.Sort(delegate (Stats left, Stats right) { return left.number.CompareTo(right.number); });

                //    System.Diagnostics.Debug.WriteLine($"-------------------- by number");

                //    foreach (var x in values)
                //        System.Diagnostics.Debug.WriteLine($"Event {x.type} num {x.number} ticks {x.ticks} total {(double)x.ticks / System.Diagnostics.Stopwatch.Frequency} per {(double)x.ticks / x.number / System.Diagnostics.Stopwatch.Frequency * 1000} ms");
                //}

                //{
                //    var values = ticks.Values.ToList();
                //    values.Sort(delegate (Stats left, Stats right) { return ((double)left.ticks / left.number).CompareTo((double)right.ticks / right.number); });

                //    System.Diagnostics.Debug.WriteLine($"-------------------- by time ");
                //    foreach (var x in values)
                //    {
                //        if (x.number > 50)
                //            System.Diagnostics.Debug.WriteLine($"Event {x.type} num {x.number} ticks {x.ticks} total {(double)x.ticks / System.Diagnostics.Stopwatch.Frequency} per {(double)x.ticks / x.number / System.Diagnostics.Stopwatch.Frequency * 1000} ms");
                //    }
                //}
                {
                    var values = ticks.Values.ToList();
                    values.Sort(delegate (Stats left, Stats right) { return left.ticks.CompareTo(right.ticks); });

                    System.Diagnostics.Debug.WriteLine($"-------------------- by ticks ");
                    foreach (var x in values)
                    {
                        System.Diagnostics.Debug.WriteLine($"Event {x.type} num {x.number} ticks {x.ticks} total {(double)x.ticks / System.Diagnostics.Stopwatch.Frequency}s per {(double)x.ticks / x.number / System.Diagnostics.Stopwatch.Frequency * 1000} ms");
                    }
                }
            }

            if (cancel.IsCancellationRequested)
            {
                return null;
            }
            return jlist;
        }

        private static void CreateJEinThread(Object o)
        {
            var data = (JETable)o;

            for (int j = data.start; j < data.end; j++)
            {
                if (j % 2000 == 0)
                {
                    if (data.iscancelled.IsCancellationRequested)       // check every X entries, if cancel is not null
                        break;
                    data.progress?.Invoke(data.processor, (j - data.start) / (float)(data.end - data.start));
                }

                var e = data.table[j];
                data.results[j] = CreateJournalEntry(e.ID, e.TLUID, e.Cmdr, e.Json, e.Syncflag);
            }

            data.progress?.Invoke(data.processor, 1.0f);
            data.finished.Signal();
        }

#endregion

#region MT process table data to a callback

        // from table data ID, travellogid, commanderid, event json, sync flag
        // create journal entries and dispatch to a thread. Entries will bombard the thread in any order, in multiple threads
        // unused idea but worth keeping
        static public void MTJournalEntries(List<TableData> tabledata, Action<JournalEntry> dispatchinthread, Func<bool> cancelRequested = null)
        {
            int threads = System.Environment.ProcessorCount / 2;        // on Rob's system 4 from 8 seems optimal, any more gives little more return

            CountdownEvent cd = new CountdownEvent(threads);
            for (int i = 0; i < threads; i++)
            {
                int s = i * tabledata.Count / threads;
                int e = (i + 1) * tabledata.Count / threads;
                Thread t1 = new Thread(new ParameterizedThreadStart(MTJEinThread));
                t1.Priority = ThreadPriority.Highest;
                t1.Name = $"MTGetAll {i}";
                System.Diagnostics.Debug.WriteLine($"MTJournal Creation Spawn {s}..{e}");
                t1.Start(new Tuple<List<TableData>, Action<JournalEntry>, int, int, CountdownEvent, Func<bool>>(tabledata, dispatchinthread, s, e, cd, cancelRequested));
            }

            cd.Wait();
        }

        private static void MTJEinThread(Object o)
        {
            var cmd = (Tuple<List<TableData>, Action<JournalEntry>, int, int, CountdownEvent, Func<bool>>)o;

            for (int j = cmd.Item3; j < cmd.Item4; j++)
            {
                if (j % 10000 == 0 && (cmd.Item6?.Invoke() ?? false))       // check every X entries
                    break;
                var e = cmd.Item1[j];
                cmd.Item2.Invoke(CreateJournalEntry(e.ID, e.TLUID, e.Cmdr, e.Json, e.Syncflag)); // and dispatch to caller
            }

            cmd.Item5.Signal();
        }

#endregion

#region Types of events

        static public Type TypeOfJournalEntry(string text)
        {
            Type t = Type.GetType(JournalRootClassname + ".Journal" + text, false, true); // no exception, ignore case here
            return t;
        }

        static public Type TypeOfJournalEntry(JournalTypeEnum type)
        {
            if (JournalEntryTypes.ContainsKey(type))
            {
                return JournalEntryTypes[type];
            }
            else
            {
                return TypeOfJournalEntry(type.ToString());
            }
        }

#endregion


#region Private variables

        protected enum SyncFlags
        {
            NoBit = 0,                      // for sync change func only
            EDSM = 0x01,
            EDDN = 0x02,
            // 0x04 was EGO
            StartMarker = 0x0100,           // measure distance start pos marker
            StopMarker = 0x0200,            // measure distance stop pos marker
        }
        private int Synced { get; set; }                     // sync flags

#endregion

#region Virtual overrides

        protected virtual JournalTypeEnum IconEventType { get { return EventTypeID; } }  // entry may be overridden to dynamically change icon event for an event

#endregion

#region Constructors

        protected JournalEntry(DateTime utc, JournalTypeEnum jtype, int syncflags = 0)       // manual creation via NEW
        {
            EventTypeID = jtype;
            EventTimeUTC = utc;
            Synced = syncflags;
            TLUId = 0;
        }

        protected JournalEntry(JObject jo, JournalTypeEnum jtype)              // called by journal entries to create themselves
        {
            EventTypeID = jtype;
            if (DateTime.TryParse(jo["timestamp"].Str(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime etime))
                EventTimeUTC = etime;
            else
                EventTimeUTC = DateTime.MinValue;
            TLUId = 0;
        }

#endregion

#region Private Type info

        private static string JournalRootClassname = typeof(JournalEvents.JournalTouchdown).Namespace;        // pick one at random to find out root classname
        private static Dictionary<JournalTypeEnum, Type> JournalEntryTypes = GetJournalEntryTypes();        // enum -> type

        // Gets the mapping of journal type value to JournalEntry type
        private static Dictionary<JournalTypeEnum, Type> GetJournalEntryTypes()
        {
            Dictionary<JournalTypeEnum, Type> typedict = new Dictionary<JournalTypeEnum, Type>();
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var types = asm.GetTypes().Where(t => typeof(JournalEntry).IsAssignableFrom(t) && !t.IsAbstract).ToList();

            foreach (Type type in types)
            {
                JournalEntryTypeAttribute typeattrib = type.GetCustomAttributes(false).OfType<JournalEntryTypeAttribute>().FirstOrDefault();
                if (typeattrib != null)
                {
                    typedict[typeattrib.EntryType] = type;
                }
            }

            return typedict;
        }

        //Activators are delegates which can make a specific JournalEntry type.  Deep c# stuff here

        private static Dictionary<string, BaseUtils.ObjectActivator.Activator<JournalEntry>> ClassActivators = GetClassActivators();

        private static Dictionary<string, BaseUtils.ObjectActivator.Activator<JournalEntry>> GetClassActivators()
        {
            var actlist = new Dictionary<string, BaseUtils.ObjectActivator.Activator<JournalEntry>> ();

            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var types = asm.GetTypes().Where(t => typeof(JournalEntry).IsAssignableFrom(t) && !t.IsAbstract).ToList();

            foreach (Type type in types)
            {
                JournalEntryTypeAttribute typeattrib = type.GetCustomAttributes(false).OfType<JournalEntryTypeAttribute>().FirstOrDefault();
                if (typeattrib != null)
                {
                    var jobjectconstructor = type.GetConstructors().Where(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType.Name == "JObject").First();
                    actlist[typeattrib.EntryType.ToString()] = BaseUtils.ObjectActivator.GetActivator<JournalEntry>(jobjectconstructor);
                 //   System.Diagnostics.Debug.WriteLine("Activator " + typeattrib.EntryType.ToString());
                }
            }

            return actlist;
        }


#endregion

#region Icons and names

        // enum -> icons 
        public static IReadOnlyDictionary<JournalTypeEnum, Image> JournalTypeIcons { get; } = new BaseUtils.Icons.IconGroup<JournalTypeEnum>("Journal");

        // enum -> Translated Name Events
        private static Dictionary<JournalTypeEnum, string> TranslatedEventNames = GetJournalTranslatedNames();     // precompute the names due to the expense of splitcapsword

        private static Dictionary<JournalTypeEnum, string> GetJournalTranslatedNames()
        {
            var v = Enum.GetValues(typeof(JournalTypeEnum)).OfType<JournalTypeEnum>();
            var tx = v.ToDictionary(e => e, 
                (ft)=> 
                {
                    // only translate ones below Obsoleteoricons
                    if ( ft < JournalTypeEnum.ObsoleteOrIcons)
                    {
                        // we need to manually fix these because they don't relate to text at all
                        string txstring = ft == JournalTypeEnum.CapShipBond ? "Capital Ship Bond" : ft == JournalTypeEnum.FCMaterials ? "Bartender Materials" : ft.ToString().SplitCapsWord();
                        return txstring.Tx();
                    }
                    else
                        return ft.ToString().SplitCapsWord(); 
                } 
                );
            return tx;
        }

#endregion

#region Helpers

        public static JObject RemoveEDDGeneratedKeys(JObject obj)      // obj not changed
        {
            JObject jcopy = null;

            foreach (var kvp in obj)
            {
                if (kvp.Key.StartsWith("EDD") || kvp.Key.Equals("StarPosFromEDSM")) // remove all EDD generated keys from json
                {
                    if (jcopy == null)      // only pay the expense if it has one of the entries in it
                        jcopy = (JObject)obj.Clone();

                    jcopy.Remove(kvp.Key);
                }
            }

            return jcopy != null ? jcopy : obj;
        }

        // optionally pass in json for speed reasons.  Guaranteed that ent1jo and 2 are not altered by the compare!
        internal static bool AreSameEntry(JournalEntry ent1, JournalEntry ent2, SQLiteConnectionUser cn, JObject ent1jo = null, JObject ent2jo = null)
        {
            if (ent1jo == null && ent1 != null)
            {
                ent1jo = GetJson(ent1.Id,cn);      // read from db the json since we don't have it
            }

            if (ent2jo == null && ent2 != null)
            {
                ent2jo = GetJson(ent2.Id,cn);      // read from db the json since we don't have it
            }

            if (ent1jo == null || ent2jo == null)
            {
                return false;
            }

            //System.Diagnostics.Debug.WriteLine("Compare " + ent1jo.ToString() + " with " + ent2jo.ToString());

            // Fixed problem #1518, Prev. the remove was only done on GetJson's above.  
            // during a scan though, ent1jo is filled in, so the remove was not being performed on ent1jo.
            // So if your current map colour was different in FSD entries then
            // the newly created entry would differ from the db version by map colour - causing #1518
            // secondly, this function should not alter ent1jo/ent2jo as its a compare function.  it was.  Change RemoveEDDGenKeys to copy if it alters it.

            JObject ent1jorm = RemoveEDDGeneratedKeys(ent1jo);     // remove keys, but don't alter originals as they can be used later 
            JObject ent2jorm = RemoveEDDGeneratedKeys(ent2jo);

            bool res = JToken.DeepEquals(ent1jorm, ent2jorm);
            //if (!res) System.Diagnostics.Debug.WriteLine("!! Not duplicate {0} vs {1}", ent1jorm.ToString(), ent2jorm.ToString()); else  System.Diagnostics.Debug.WriteLine("Duplicate");
            return res;
        }

        protected JObject ReadAdditionalFile( string extrafile, string eventnametocheck )       // read file, return new JSON
        {
            for (int retries = 0; retries < 25*4 ; retries++)
            {
                // this has the full version of the event, including data, at the same timestamp
                string json = BaseUtils.FileHelpers.TryReadAllTextFromFile(extrafile);      // null if not there, or locked..

                // decode into JObject if there, null if in error or not there
                JObject joaf = json != null ? JObject.Parse(json, JToken.ParseOptions.AllowTrailingCommas | JToken.ParseOptions.CheckEOL) : null;

                if (joaf != null)
                {
                    string newtype = joaf["event"].Str();
                    DateTime fileUTC = joaf["timestamp"].DateTimeUTC();
                    if (newtype != eventnametocheck || fileUTC == DateTime.MinValue)
                    {
                        System.Diagnostics.Debug.WriteLine($"Rejected {extrafile} due to type/bad date, deleting");
                        BaseUtils.FileHelpers.DeleteFileNoError(extrafile);     // may be corrupt..
                        return null;
                    }
                    else
                    {
                        if (fileUTC > EventTimeUTC)
                        {
                          //  System.Diagnostics.Debug.WriteLine($"File is younger than Event, can't be associated {extrafile}");
                            return null;
                        }
                        else if (fileUTC == EventTimeUTC)
                        {
                            System.Diagnostics.Debug.WriteLine($"Read {extrafile} at {fileUTC} after {retries}");
                            return joaf;                        // good current file..
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"File not written yet due to timestamp {extrafile} at {fileUTC}, waiting.. {retries}");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot read {extrafile}, waiting.. {retries}");
                }

                System.Threading.Thread.Sleep(25);
            }

            return null;
        }

#endregion

    }

    #region Attributes of a JournalEntry

    [AttributeUsage(AttributeTargets.Class)]
    public class JournalEntryTypeAttribute : Attribute
    {
        public JournalTypeEnum EntryType { get; set; }

        public JournalEntryTypeAttribute(JournalTypeEnum entrytype)
        {
            EntryType = entrytype;
        }
    }

    #endregion
}

