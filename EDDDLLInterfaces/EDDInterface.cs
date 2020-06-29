/*
 * Copyright © 2015 - 2020 EDDiscovery development team
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

using System;
using System.Runtime.InteropServices;

namespace EDDDLLInterfaces
{
    public static class EDDDLLIF       // Standard DLL Interface for a C++ type program
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct JournalEntry
        {
            [FieldOffset(0)] public int ver;
            [FieldOffset(4)] public int indexno;
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

            [FieldOffset(88)] public double travelleddistance;
            [FieldOffset(96)] public long travelledseconds;

            [FieldOffset(100)] public bool islanded;
            [FieldOffset(101)] public bool isdocked;

            [FieldOffset(104)] [MarshalAs(UnmanagedType.BStr)] public string whereami;
            [FieldOffset(112)] [MarshalAs(UnmanagedType.BStr)] public string shiptype;
            [FieldOffset(120)] [MarshalAs(UnmanagedType.BStr)] public string gamemode;
            [FieldOffset(128)] [MarshalAs(UnmanagedType.BStr)] public string group;
            [FieldOffset(136)] public long credits;

            [FieldOffset(144)] [MarshalAs(UnmanagedType.BStr)] public string eventid;

            [FieldOffset(152)] [MarshalAs(UnmanagedType.SafeArray)] public string[] currentmissions;

            [FieldOffset(160)] public long jid;
            [FieldOffset(168)] public int totalrecords;

            // Version 1 Ends here

            [FieldOffset(176)] [MarshalAs(UnmanagedType.BStr)] public string json;
            [FieldOffset(184)] [MarshalAs(UnmanagedType.BStr)] public string cmdrname;
            [FieldOffset(192)] [MarshalAs(UnmanagedType.BStr)] public string cmdrfid;
            [FieldOffset(200)] [MarshalAs(UnmanagedType.BStr)] public string shipident;
            [FieldOffset(208)] [MarshalAs(UnmanagedType.BStr)] public string shipname;
            [FieldOffset(216)] public long hullvalue;
            [FieldOffset(224)] public long rebuy;
            [FieldOffset(232)] public long modulesvalue;
            [FieldOffset(240)] public bool stored;          // true if its a stored replay journal, false if live

            // Version 2 Ends here
        };

        public delegate bool EDDRequestHistory(long index, bool isjid, out JournalEntry f); //index =1..total records, or jid
        public delegate bool EDDRunAction([MarshalAs(UnmanagedType.BStr)]string eventname,
                                             [MarshalAs(UnmanagedType.BStr)]string parameters);  // parameters in format v="k",X="k"

        [return: MarshalAs(UnmanagedType.BStr)]
        public delegate string EDDShipLoadout(string name); //index =1..total records, or jid


        [StructLayout(LayoutKind.Explicit)]
        public struct EDDCallBacks
        {
            [FieldOffset(0)] public int ver;
            [FieldOffset(8)] public EDDRequestHistory RequestHistory;
            [FieldOffset(16)] public EDDRunAction RunAction;
            // Version 1 Ends here
            [FieldOffset(24)] public EDDShipLoadout GetShipLoadout;
            // Version 2 Ends here

        }

        // c# assemblies implement the following functions inside a class named *MainDLL.  This is the class which is instanced
        // and gets EDDInitialise, EDDTerminate etc called on it with the parameters as below.

        // C++ DLLs implement these functions with the types indicated in the MarshalAs

        public const string FLAG_HOSTNAME = "HOSTNAME=";                    // flags in
        public const string FLAG_JOURNALVERSION = "JOURNALVERSION=";
        public const string FLAG_CALLBACKVERSION = "CALLBACKVERSION=";

        public const string FLAG_PLAYLASTFILELOAD = "PLAYLASTFILELOAD";     // flags back

        // Manadatory
        // vstr = Host Vnum [;InOptions]..
        //      HOSTNAME=x
        //      JOURNALVERSION=x
        // return !errorstring || DLLVNumber [;RetOptions]..
        // DLLVnumber = 0.0.0.0 [;OutOptions]
        //      PLAYLASTFILELOAD - play the start events on Commander refresh Fileheader, Commander, Materials, LoadGame, Rank, Progress, reputation, EngineerProgress, Location, Missions

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.BStr)]     
        public delegate String EDDInitialise([MarshalAs(UnmanagedType.BStr)]string vstr,        
                                             [MarshalAs(UnmanagedType.BStr)]string dllfolder,
                                             EDDCallBacks callbacks);

        // Manadatory
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EDDTerminate();

        // Optional
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EDDRefresh([MarshalAs(UnmanagedType.BStr)]string cmdname, JournalEntry lastje);

        // Optional
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
        public delegate void EDDNewUIEvent([MarshalAs(UnmanagedType.BStr)]string jsonui);

        // Optional 
        // back: list of (config name, config value, config type (string,int)) of all configs
        // in : either an empty array or list of (name, values) to set
        // if fails, return empty array back
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.SafeArray)]
        public delegate string[] EDDConfigParameters([MarshalAs(UnmanagedType.SafeArray)] string[] values);

        // Version 2 Ends here

    }

}
