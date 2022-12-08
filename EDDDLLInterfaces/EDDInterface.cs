/*
 * Copyright © 2015 - 2022 EDDiscovery development team
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

using System.Drawing;
using System;
using System.Runtime.InteropServices;

namespace EDDDLLInterfaces
{
    public static class EDDDLLIF      
    {
        #region Journal Structure

        [StructLayout(LayoutKind.Explicit)]
        public struct JournalEntry
        {
            // offsets apply to WIN32 DLL
            // int/long = 4 offset, aligned 4
            // bool = 1 offset, next aligned to 1, first aligned 4
            // BSTR/Safearray = 8 offset, aligned 8

            [FieldOffset(0)] public int ver;
            [FieldOffset(4)] public int indexno;        // if -1, null record, rest is invalid.  For NewJournalEntry, its HL position (0..). Not valid for NewUnfilteredJournalEntry

            [FieldOffset(8)] [MarshalAs(UnmanagedType.BStr)] public string utctime;
            [FieldOffset(16)] [MarshalAs(UnmanagedType.BStr)] public string name;
            [FieldOffset(24)] [MarshalAs(UnmanagedType.BStr)] public string info;
            [FieldOffset(32)] [MarshalAs(UnmanagedType.BStr)] public string detailedinfo;

            [FieldOffset(40)] [MarshalAs(UnmanagedType.SafeArray)] public string[] materials;
            [FieldOffset(48)] [MarshalAs(UnmanagedType.SafeArray)] public string[] commodities;

            [FieldOffset(56)] [MarshalAs(UnmanagedType.BStr)] public string systemname;
            [FieldOffset(64)] public double x;
            [FieldOffset(72)] public double y;
            [FieldOffset(80)] public double z;

            [FieldOffset(88)] public double travelleddistance;      // from start/stop flags
            [FieldOffset(96)] public long travelledseconds;

            [FieldOffset(100)] public bool islanded;
            [FieldOffset(101)] public bool isdocked;

            [FieldOffset(104)] [MarshalAs(UnmanagedType.BStr)] public string whereami;
            [FieldOffset(112)] [MarshalAs(UnmanagedType.BStr)] public string shiptype;  // nice name, Unknown not set
            [FieldOffset(120)] [MarshalAs(UnmanagedType.BStr)] public string gamemode; // Unknown not set
            [FieldOffset(128)] [MarshalAs(UnmanagedType.BStr)] public string group; // empty if not group
            [FieldOffset(136)] public long credits;

            [FieldOffset(144)] [MarshalAs(UnmanagedType.BStr)] public string eventid;

            [FieldOffset(152)] [MarshalAs(UnmanagedType.SafeArray)] public string[] currentmissions;

            [FieldOffset(160)] public long jid;
            [FieldOffset(168)] public int totalrecords;     // Number of HLs for NewJournalEntry, for Unfiltered its no of HLs before add.

            // Version 1 Ends here

            [FieldOffset(176)] [MarshalAs(UnmanagedType.BStr)] public string json;
            [FieldOffset(184)] [MarshalAs(UnmanagedType.BStr)] public string cmdrname;
            [FieldOffset(192)] [MarshalAs(UnmanagedType.BStr)] public string cmdrfid;
            [FieldOffset(200)] [MarshalAs(UnmanagedType.BStr)] public string shipident;     // if not known "Unknown" is used
            [FieldOffset(208)] [MarshalAs(UnmanagedType.BStr)] public string shipname;     // if not known "Unknown" is used
            [FieldOffset(216)] public long hullvalue;       // offsets here are not right for the thunk to a WIN32 DLL. should have been 8.  C# will see a long, c++ will see a uint
            [FieldOffset(220)] public long rebuy;
            [FieldOffset(224)] public long modulesvalue;
            [FieldOffset(228)] public bool stored;          // true if its a stored replay journal, false if live

            // Version 2 Ends here

            [FieldOffset(232)] [MarshalAs(UnmanagedType.BStr)] public string travelstate;
            [FieldOffset(240)] [MarshalAs(UnmanagedType.SafeArray)] public string[] microresources;

            // Version 3 Ends here

            [FieldOffset(248)] public bool horizons;
            [FieldOffset(249)] public bool odyssey;
            [FieldOffset(250)] public bool beta;

            // Version 4 Ends here

            [FieldOffset(251)] public bool wanted;
            [FieldOffset(252)] public bool bodyapproached;
            [FieldOffset(253)] public bool bookeddropship;
            [FieldOffset(254)] public bool issrv;
            [FieldOffset(255)] public bool isfighter;
            [FieldOffset(256)] public bool onfoot;
            [FieldOffset(257)] public bool bookedtaxi;
            [FieldOffset(264)] [MarshalAs(UnmanagedType.BStr)] public string bodyname;          // if not known "Unknown" is used, or may be blank
            [FieldOffset(272)] [MarshalAs(UnmanagedType.BStr)] public string bodytype;          // if not known "Unknown" is used, or may be blank
            [FieldOffset(280)] [MarshalAs(UnmanagedType.BStr)] public string stationname;       // if not known "Unknown" is used, or may be blank
            [FieldOffset(288)] [MarshalAs(UnmanagedType.BStr)] public string stationtype;       // if not known "Unknown" is used, or may be blank
            [FieldOffset(296)] [MarshalAs(UnmanagedType.BStr)] public string stationfaction;    // if not known "Unknown" is used, or may be blank
            [FieldOffset(304)] [MarshalAs(UnmanagedType.BStr)] public string shiptypefd;        // if not known "Unknown" is used, or may be blank
            [FieldOffset(312)] [MarshalAs(UnmanagedType.BStr)] public string oncrewwithcaptain;    // empty not in multiplayer
            [FieldOffset(320)] public ulong shipid;        // ulong.maxvalue = unknown
            [FieldOffset(328)] public int bodyid;        //  -1 not on body

            // Version 5 Ends here

            [FieldOffset(336)] [MarshalAs(UnmanagedType.BStr)] public string gameversion;
            [FieldOffset(344)] [MarshalAs(UnmanagedType.BStr)] public string gamebuild;

            // Version 6 Ends here (16.0.4 Dec 22)
        };

        #endregion


        #region Callbacks

        /// Callbacks - Host uses CALLBACKVERSION to tell DLL what version it implements

        [StructLayout(LayoutKind.Explicit)]
        public struct EDDCallBacks
        {
            public delegate bool EDDRequestHistory(long index, bool isjid, out JournalEntry f); //index =1..total records, or jid
            public delegate bool EDDRunAction([MarshalAs(UnmanagedType.BStr)] string eventname,
                                                 [MarshalAs(UnmanagedType.BStr)] string parameters);  // parameters in format v="k",X="k"

            [return: MarshalAs(UnmanagedType.BStr)]
            public delegate string EDDShipLoadout([MarshalAs(UnmanagedType.BStr)] string name); //index =1..total records, or jid

            public delegate void EDDAddPanel(string id, Type paneltype, string wintitle, string refname, string description, System.Drawing.Image img);

            [FieldOffset(0)] public int ver;
            [FieldOffset(8)] public EDDRequestHistory RequestHistory;
            [FieldOffset(16)] public EDDRunAction RunAction;
            // Version 1 Ends here
            [FieldOffset(24)] public EDDShipLoadout GetShipLoadout;
            // Version 2 Ends here
            [FieldOffset(32)] public EDDAddPanel AddPanel;          // c# only DLLs, may be null. Check both ver and if non null. Only valid during EDDInitialise
                                                                    // give an id name globally unique, like "author-panel-version"
            // Version 3 Ends here (16.0.4 Dec 22)
        }

        // This class is passed to panel on Init.
        public class EDDPanelCallbacks
        {
            public int ver;

            public delegate void PanelSave<T>(string key, T value);
            public delegate T PanelGet<T>(string key, T defvalue);
            public delegate void PanelSaveGridLayout(object grid, string auxkey = "");
            public delegate void PanelLoadGridLayout(object grid, string auxkey = "");
            public delegate bool PanelBool();
            public delegate void PanelString(string s);
            public delegate void PanelDGVTransparent(object grid, bool on, Color curcol);
            public delegate JournalEntry PanelJournalEntry(int index);
            public delegate Tuple<string, double, double, double> PanelGetTarget();

            public PanelSave<string> SaveString;
            public PanelSave<double> SaveDouble;
            public PanelSave<long> SaveLong;
            public PanelSave<int> SaveInt;
            public PanelGet<string> GetString;
            public PanelGet<double> GetDouble;
            public PanelGet<long> GetLong;
            public PanelGet<int> GetInt;
            public PanelSaveGridLayout SaveGridLayout;
            public PanelLoadGridLayout LoadGridLayout;
            public PanelString SetControlText;
            public PanelBool HasControlTextArea;
            public PanelBool IsControlTextVisible;
            public PanelBool IsTransparentModeOn;       // is transparent mode allowed? (does not mean its currently transparent)
            public PanelBool IsFloatingWindow;          // is it a floating window outside (in a form)
            public PanelBool IsClosed;                  // very important if your doing async programming - the window may have closed when the async returns!
            public PanelDGVTransparent DGVTransparent;  // Theme the DGV with transparency or not
            public PanelJournalEntry GetHistoryEntry;   // if index is out of range, you get a history entry with indexno = -1. Returns filtered history
            public PanelGetTarget GetTarget;            // null if target is not set
            public PanelString WriteToLog;
            public PanelString WriteToLogHighlight;

            // ver 1 ends
        };

        public interface IEDDPanelExtension                         // an external panel implements this interface
        {
            // Theme is json per ExtendedControls.Theme.  Make sure you cope with any changes we make - don't presume a key is there. Be defensive
            // configuration is for future use
            void Initialise(EDDPanelCallbacks callbacks, string themeasjson, string configuration);
            void SetTransparency(bool ison, Color curcol);          // called when transparency changes. curcol is the colour you should apply to back color of controls
            void LoadLayout();
            void InitialDisplay();
            void CursorChanged(JournalEntry je);                    // fed thru when the external panel history cursor changes
            void Closing();
            bool SupportTransparency { get; }
            bool DefaultTransparent { get; }
            bool AllowClose();
            string HelpKeyOrAddress();
            void ControlTextVisibleChange(bool on);

            // event interface
            void HistoryChange(int count, string commander, bool beta, bool legacy);        // when a history is loaded
            void NewUnfilteredJournal(JournalEntry je);
            void NewFilteredJournal(JournalEntry je);
            void NewUIEvent(string jsonui);     // see UIEvent and subclasses in EliteDangerousCore - has EventTimeUTC, EventTypeID, EventTypeStr, EventRefresh plus fields unique to EventType
            void NewTarget(Tuple<string, double, double, double> target);    // null if target has been removed
            void ScreenShotCaptured(string file, Size s);
            void ThemeChanged(string themeasjson);
        }

        #endregion

        #region Calls to DLL

        // c# assemblies implement the following functions inside a class named *MainDLL.  This is the class which is instanced
        // and gets EDDInitialise, EDDTerminate etc called on it with the parameters as below.
        // C++ DLLs implement these functions with the types indicated in the MarshalAs

        // Flags passed by EDDInitialise in vstr:

        public const string FLAG_HOSTNAME = "HOSTNAME=";                    // aka EDDiscovery, EDDLite
        public const string FLAG_JOURNALVERSION = "JOURNALVERSION=";        // see EDDDLLHistoryEntry.cs for JournalVersion, version of the journal implemented by host
        public const string FLAG_CALLBACKVERSION = "CALLBACKVERSION=";      // see EDDCallBacks.cs, callback version implemented by host for the structure EDDCallBacks
        public const string FLAG_CALLVERSION = "CALLVERSION=";              // The version of the DLL interface the host implements (see CallVersion for the best)

        public const string FLAG_PLAYLASTFILELOAD = "PLAYLASTFILELOAD";     // flags back

        // Init. Called by Discoveryform Load - system is not fully up at this point. History is not available etc. No callbacks work
        // Mandatory
        // vstr = Host Vnum [;InOptions]..
        //      HOSTNAME=x
        //      JOURNALVERSION=x
        //      CALLBACKVERSION=x
        //      CALLVERSION=x
        // return !errorstring || DLLVNumber [;RetOptions]..
        // DLLVnumber = 0.0.0.0 [;OutOptions]
        //      PLAYLASTFILELOAD - play the start events on Commander refresh Fileheader, Commander, Materials, LoadGame, Rank, Progress, reputation, EngineerProgress, Location, Missions

        // Manadatory
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.BStr)]     
        public delegate String EDDInitialise([MarshalAs(UnmanagedType.BStr)]string vstr,               
                                             [MarshalAs(UnmanagedType.BStr)]string dllfolder,
                                             EDDCallBacks callbacks);

        // Manadatory
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EDDTerminate();

        // Optional, called when history has been accumulated.  From now on callbacks can be used
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EDDRefresh([MarshalAs(UnmanagedType.BStr)]string cmdname, JournalEntry lastje);

        // Optional, this is the JEs EDDiscovery main system sees, post filtering reordering
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EDDNewJournalEntry(JournalEntry nje);      

        // Optional DLLCall in Action causes this
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.BStr)]                         // paras can be an empty array, but is always present
        public delegate String EDDActionCommand([MarshalAs(UnmanagedType.BStr)]string cmdname, [MarshalAs(UnmanagedType.SafeArray)]string[] paras);

        // Optional DLLCall in Action causes this with a JID
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
        public delegate void EDDActionJournalEntry(JournalEntry lastje);

        // Version 1 Ends here

        // Optional
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EDDNewUIEvent([MarshalAs(UnmanagedType.BStr)] string jsonui);

        // config parameters has been removed - never used

        // Version 2 Ends here

        // Version 5 

        // Optional, Configure event, called just after EDDInitialise(), to pass in any config string that the system has saved for you
        // string passed back is saved by system
        // if editit = true, user wants you to offer the option to change the config
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.BStr)]                         
        public delegate String EDDConfig([MarshalAs(UnmanagedType.BStr)] string input, bool editit);

        // Optional
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EDDNewUnfilteredJournalEntry(JournalEntry nje);      // unfiltered stream of JEs before any ordering. Note list number is not applicable

        // version 5 ends here

        // version 6

        // Optional
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EDDMainFormShown();                                    // after main form is shown

        // version 6 ends

        public const int CallBackVersion = 6;

        #endregion
    }

}
