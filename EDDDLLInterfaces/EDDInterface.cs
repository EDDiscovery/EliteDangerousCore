/*
 * Copyright © 2015 - 2024 EDDiscovery development team
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
using static EDDDLLInterfaces.EDDDLLIF;

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
            // string = 8 offset, aligned 8
            // BSTR/Safearray = 8 offset, aligned 8

            [FieldOffset(0)] public int ver;            // indicate version (JOURNALVERSION in init string)
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

            [FieldOffset(352)] [MarshalAs(UnmanagedType.BStr)] public string fsdjumpnextsystemname;     // empty if not set
            [FieldOffset(360)] public long fsdjumpnextsystemaddress;    // 0 if not set
            [FieldOffset(368)] public long systemaddress;               // 0 if not known
            [FieldOffset(376)] public long marketid;                    // 0 if not known
            [FieldOffset(384)] public long fullbodyid;                  // 0 if not known    
            [FieldOffset(392)] public long loan;
            [FieldOffset(400)] public long assets;
            [FieldOffset(408)] public double currentboost;
            [FieldOffset(416)] public int visits;
            [FieldOffset(420)] public bool multiplayer;
            [FieldOffset(421)] public bool insupercruise;

            // Version 7 Ends here (17.2 24/4/24)
        };

        public const int JournalVersion = 7;

        #endregion


        #region Callbacks

        /// Callbacks - CALLBACKVERSION is used in Initialise to tell DLL what version it implements or use ver
        /// Do not use tuple or complicated generic returns/parameters as the marshaller with win32 dll does barfe on them!

        public delegate bool EDDRequestHistory(long index, bool isjid, out JournalEntry f); //index =1..total records, or jid
        public delegate bool EDDRunAction([MarshalAs(UnmanagedType.BStr)] string eventname,
                                             [MarshalAs(UnmanagedType.BStr)] string parameters);  // parameters in format v="k",X="k"

        [return: MarshalAs(UnmanagedType.BStr)]
        public delegate string EDDShipLoadout([MarshalAs(UnmanagedType.BStr)] string name); //index =1..total records, or jid

        public delegate void EDDAddPanel(string id, Type paneltype, string wintitle, string refname, string description, System.Drawing.Image img);

        public delegate void EDDString([MarshalAs(UnmanagedType.BStr)] string s);

        [return: MarshalAs(UnmanagedType.BStr)] 
        public delegate string EDDReturnString();
        [return: MarshalAs(UnmanagedType.BStr)]
        public delegate string EDDVisitedList(int number);

        // transparent change 15/1/25 the bool now means spansh then edsm
        public delegate void EDDRequestScanData(object requesttag, object usertag, [MarshalAs(UnmanagedType.BStr)] string system, bool spanshthenedsmlookup);

        // New interfaces for version 4. 

        public delegate void EDDRequestScanDataExt(object requesttag, object usertag, [MarshalAs(UnmanagedType.BStr)] string system, long systemaddress, 
                                        int weblookup, [MarshalAs(UnmanagedType.BStr)] string otheroptions);

        public delegate string EDDRequestGMOs(string requestype);

        // ALL CALLBACKS must be called on UI thread.

        [StructLayout(LayoutKind.Explicit)]
        public struct EDDCallBacks
        {
            [FieldOffset(0)] public int ver;                        // version (same as CALLBACKVERSION)

            // DLL get history entry by index or jid. If program does not have history or out of range, return false
            [FieldOffset(8)] public EDDRequestHistory RequestHistory;

            // DLL run an action script. If program does not have action, return false.
            [FieldOffset(16)] public EDDRunAction RunAction;        

            // Version 1 Ends here

            // get loadout as JSON string from ShipInformation
            // ALL returns objects (0-N) of all ships known
            // "" returns current ship
            // "Name" "Ident" returns specific ship
            // may return null - not known or not supported
            [FieldOffset(24)] public EDDShipLoadout GetShipLoadout;

            // Version 2 Ends here
            // c# only DLLs, may be null. Check both ver and if non null. Only valid during EDDInitialise
            // give an id name globally unique, like "author-panelname"
            [FieldOffset(32)] public EDDAddPanel AddPanel;

            // Return JSON of target,X,Y,Z as an object, or empty object
            [FieldOffset(40)] public EDDReturnString GetTarget;        
            
            [FieldOffset(48)] public EDDString WriteToLog;
            [FieldOffset(56)] public EDDString WriteToLogHighlight;

            // the following are JSON outputs of EDD structures - structures may change. DLLs will have to keep up. Be defensive.

            // c# only Request a Scan Data
            // Get StarNode structure with ScanData on all bodies in a system. Empty string for current, else star system.  
            // when ready, you will receive a EDDDataResult
            [FieldOffset(64)] public EDDRequestScanData RequestScanData;

            // Get suit, weapons and loadout structures, current state.
            [FieldOffset(72)] public EDDReturnString GetSuitsWeaponsLoadouts;   

            // Get carrier data structure, current state.
            [FieldOffset(80)] public EDDReturnString GetCarrierData;   

            // Get last N visited systems. N may be bigger than list, in which case all are returned. <0 means all. Expensive in time - do not use frequently.
            [FieldOffset(88)] public EDDVisitedList GetVisitedList;

            // Get shipyards visited with data. Expensive in time - do not use frequently.
            [FieldOffset(96)] public EDDReturnString GetShipyards;

            // Get Outfittings visited with data. Expensive in time - do not use frequently.
            [FieldOffset(104)] public EDDReturnString GetOutfitting;

            // Version 3 Ends here (16.0.4 Dec 22)

            // Get GMO Objects 
            // requesttype = All, Visible, Name=<wildcard name>, SystemName=<name>, Types=<typelist>
            [FieldOffset(112)] public EDDRequestGMOs GetGMOs;

            // Get Scan Data
            // System = "name" systemaddress=0 valid
            // System = "" systemaddress=N valid
            // System = "name" systemaddress=N valid with systemaddress preferred as the lookup source
            // System = "" systemaddress = 0 get current system information
            // web lookup = 3 SpanshThenEDSM 2 = Spansh 1 = EDSM 0 = None
            // otheroptions is unused as of now.

            [FieldOffset(120)] public EDDRequestScanDataExt RequestScanDataExt;

            // Version 4 Ends here (19.0 Jan 24)
        }

        public const int DLLCallBackVersion = 4;

        // ALL CALLBACKS must be called on UI thread.
        // This class is passed to panel on Init.
        // Must be fixed once released.  Use an EDDPanelCallBacks2 etc interface to expand later, and you'll need to add a new ExtPanel host

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
            public delegate bool PanelPushStarsList(string panelname, System.Collections.Generic.List<string> stars);
            public delegate bool PanelPushCSV(string filename);

            public PanelSave<string> SaveString;        // thread safe
            public PanelSave<double> SaveDouble;        // thread safe
            public PanelSave<long> SaveLong;            // thread safe
            public PanelSave<int> SaveInt;              // thread safe
            public PanelGet<string> GetString;          // thread safe
            public PanelGet<double> GetDouble;          // thread safe
            public PanelGet<long> GetLong;              // thread safe
            public PanelGet<int> GetInt;                // thread safe
            public PanelSaveGridLayout SaveGridLayout;  // UI Thread only
            public PanelLoadGridLayout LoadGridLayout;  // UI Thread only
            public PanelString SetControlText;          // UI Thread only
            public PanelBool HasControlTextArea;        // thread safe
            public PanelBool IsControlTextVisible;      // thread safe
            public PanelBool IsTransparentModeOn;       // thread safe : is transparent mode allowed? (does not mean its currently transparent)
            public PanelBool IsFloatingWindow;          // thread safe : is it a floating window outside (in a form)
            public PanelBool IsClosed;                  // thread safe : very important if your doing async programming - the window may have closed when the async returns!
            public PanelDGVTransparent DGVTransparent;  // UI Thread only : Theme the DGV with transparency or not
            public PanelBool RequestTravelGridPosition;    // UI Thread only : ask for the travel grid position to be sent via CursorChanged
            public PanelPushStarsList PushStars;        // UI Thread only : push a star list to "triwanted","trisystems" or "expedition".
            public PanelPushCSV PushCSVToExpedition;    // UI Thread only : push a CSV file to the expedition panel

            // ver 1 ends
        };


        public const int PanelCallBackVersion = 1;

        // an external panel implements this interface.  You need to match your DLL to this interface
        public interface IEDDPanelExtension
        {
            // Theme is json per ExtendedControls.Theme.  Make sure you cope with any changes we make - don't presume a key is there. Be defensive
            // displayid is a number given to identify an unique panel - just use it if your going to store per window configuration
            // outside of the callback Get functions. Do not derive any other info from it - the meaning of number may change in future
            // configuration is for future use
            void Initialise(EDDPanelCallbacks callbacks, int displayid, string themeasjson, string configuration);
            void SetTransparency(bool ison, Color curcol);          // called when transparency changes. curcol is the colour you should apply to back color of controls
            void LoadLayout();
            void InitialDisplay();
            void CursorChanged(JournalEntry je);                    // fed thru when the external panel history cursor changes
            void Closing();
            bool SupportTransparency { get; }
            bool DefaultTransparent { get; }
            void TransparencyModeChanged(bool on);
            bool AllowClose();
            string HelpKeyOrAddress();
            void ControlTextVisibleChange(bool on);

            // event interface
            void HistoryChange(int count, string commander, bool beta, bool legacy);        // when a history is loaded
            void NewUnfilteredJournal(JournalEntry je);
            void NewFilteredJournal(JournalEntry je);
            void NewUIEvent(string jsonui);     // see UIEvent and subclasses in EliteDangerousCore - has EventTimeUTC, EventTypeID, EventTypeStr, EventRefresh plus fields unique to EventType
            void NewTarget(Tuple<string, double, double, double> target);    // null if target has been removed

            // other callbacks
            void ScreenShotCaptured(string file, Size s);
            void ThemeChanged(string themeasjson);
        }

        // for the future. We would declare an extension class, we would create a second UserControlExtPanel handling this
        // the type passed in AddPanel gives you the interface it needs
        // public interface IEDDPanelExtensionV2 : IEDDPanelExtension          // Example extension - we would declare a extension class

#endregion

#region Calls to DLL

        // c# assemblies implement the following functions inside a class named *MainDLL.  This is the class which is instanced
        // and gets EDDInitialise, EDDTerminate etc called on it with the parameters as below.
        // C++ DLLs implement these functions with the types indicated in the MarshalAs

        // Flags passed by EDDInitialise in vstr:

        public const string FLAG_HOSTNAME = "HOSTNAME=";                    // aka EDDiscovery, EDDLite
        public const string FLAG_JOURNALVERSION = "JOURNALVERSION=";        // JournalVersion: version of the journal implemented by host for structure JournalEntry
        public const string FLAG_CALLBACKVERSION = "CALLBACKVERSION=";      // DLLCallBackVersion: version implemented by host for the structure EDDCallBacks
        public const string FLAG_CALLVERSION = "CALLVERSION=";              // The version of the DLL interface the host implements (see CallerVersion for the best)
        public const string FLAG_PANELCALLBACKVERSION = "PANELCALLBACKVERSION=";   // PanelCallBackVersion: Callback version implemented by host for panels. Missing if no panel support.

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

        // Optional (TBD if it works in c++)
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EDDDataResult(object requesttag, object usertag, [MarshalAs(UnmanagedType.BStr)] string data);  // call back from RequestScanData callback and maybe more in future

        // version 7 ends
        public const int CallerVersion = 7;

#endregion
    }

}
