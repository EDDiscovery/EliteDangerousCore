/*
 * Copyright © 2015 - 2021 EDDiscovery development team
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

namespace EliteDangerousCore.DLL
{
    public class EDDDLLCaller
    {
        public string Version { get; private set; }
        public string[] DLLOptions { get; private set; }
        public string Name { get; private set; }

        // for a standard DLL
        private IntPtr pDll = IntPtr.Zero;
        private IntPtr pNewJournalEntry = IntPtr.Zero;
        private IntPtr pNewUnfilteredJournalEntry = IntPtr.Zero;
        private IntPtr pActionJournalEntry = IntPtr.Zero;
        private IntPtr pActionCommand = IntPtr.Zero;
        private IntPtr pConfig = IntPtr.Zero;
        private IntPtr pNewUIEvent = IntPtr.Zero;
        private IntPtr pShown = IntPtr.Zero;

        // for a csharp assembly
        private dynamic AssemblyMainType;

        public bool Load(string path)
        {
            if (pDll == IntPtr.Zero && AssemblyMainType == null)
            {
                try
                {       // try to load csharp assembly  - exception if not a compatible dll
                    System.Reflection.AssemblyName.GetAssemblyName(path);        // this excepts quicker on C++ DLLs than LoadFrom

                    var asm = System.Reflection.Assembly.LoadFrom(path);        // load into our context - we load all assemblies in the folder

                    var types = asm.GetTypes();

                    foreach (var type in types)         // NOTE assembly dependencies may cause AppDomain.AssemblyResolve - handle it
                    {
                        if (type.IsClass && type.FullName.EndsWith("EDDClass"))
                        {
                            System.Diagnostics.Debug.WriteLine("Type " + type.FullName);
                            AssemblyMainType = Activator.CreateInstance(type);
                            Name = System.IO.Path.GetFileNameWithoutExtension(path);
                            return true;
                        }
                    }
                }
                catch
                {
                    pDll = BaseUtils.Win32.UnsafeNativeMethods.LoadLibrary(path);

                    if (pDll != IntPtr.Zero)
                    {
                        IntPtr peddinit = BaseUtils.Win32.UnsafeNativeMethods.GetProcAddress(pDll, "EDDInitialise");

                        if (peddinit != IntPtr.Zero)        // must have this to be an EDD DLL
                        {
                            Name = System.IO.Path.GetFileNameWithoutExtension(path);
                            pNewJournalEntry = BaseUtils.Win32.UnsafeNativeMethods.GetProcAddress(pDll, "EDDNewJournalEntry");
                            pNewUnfilteredJournalEntry = BaseUtils.Win32.UnsafeNativeMethods.GetProcAddress(pDll, "EDDNewUnfilteredJournalEntry");
                            pActionJournalEntry = BaseUtils.Win32.UnsafeNativeMethods.GetProcAddress(pDll, "EDDActionJournalEntry");
                            pActionCommand = BaseUtils.Win32.UnsafeNativeMethods.GetProcAddress(pDll, "EDDActionCommand");
                            pConfig = BaseUtils.Win32.UnsafeNativeMethods.GetProcAddress(pDll, "EDDConfig");
                            pNewUIEvent = BaseUtils.Win32.UnsafeNativeMethods.GetProcAddress(pDll, "EDDNewUIEvent");
                            pShown = BaseUtils.Win32.UnsafeNativeMethods.GetProcAddress(pDll, "EDDMainFormShown");
                            return true;
                        }
                        else
                        {
                            BaseUtils.Win32.UnsafeNativeMethods.FreeLibrary(pDll);
                            pDll = IntPtr.Zero;
                        }
                    }
                }
            }

            return false;
        }


        public bool Init(string ourversion, string[] optioninlist, string dllfolder, EDDDLLInterfaces.EDDDLLIF.EDDCallBacks callbacks)
        {
            string strto = ourversion + (optioninlist != null ? (';' + String.Join(";", optioninlist)) : "");
            if (AssemblyMainType != null)
            {
                Version = AssemblyMainType.EDDInitialise(strto, dllfolder, callbacks);
            }
            else if (pDll != IntPtr.Zero)
            {
                IntPtr peddinit = BaseUtils.Win32.UnsafeNativeMethods.GetProcAddress(pDll, "EDDInitialise");

                EDDDLLInterfaces.EDDDLLIF.EDDInitialise edinit = (EDDDLLInterfaces.EDDDLLIF.EDDInitialise)Marshal.GetDelegateForFunctionPointer(
                                                                                                peddinit,
                                                                                                typeof(EDDDLLInterfaces.EDDDLLIF.EDDInitialise));
                Version = edinit(strto, dllfolder, callbacks);
            }
            else
                Version = "!";

            bool ok = Version != null && Version.Length > 0 && Version[0] != '!';

            if (ok)
            {
                var list = Version.Split(';');
                Version = list[0];
                if (list.Length > 1)
                {
                    DLLOptions = new string[list.Length - 1];
                    Array.Copy(list, 1, DLLOptions, 0, list.Length - 1);
                }
                else
                    DLLOptions = new string[] { };
                return true;
            }
            else
            {
                if (pDll != IntPtr.Zero)
                {
                    BaseUtils.Win32.UnsafeNativeMethods.FreeLibrary(pDll);
                    pDll = IntPtr.Zero;
                }
            }

            return false;
        }

        public bool UnLoad()
        {
            if (AssemblyMainType != null)
            {
                AssemblyMainType.EDDTerminate();
                AssemblyMainType = null;
                Version = null;
                DLLOptions = null;
                return true;
            }
            else if (pDll != IntPtr.Zero)
            {
                IntPtr pAddressOfFunctionToCall = BaseUtils.Win32.UnsafeNativeMethods.GetProcAddress(pDll, "EDDTerminate");

                if (pAddressOfFunctionToCall != IntPtr.Zero)
                {
                    EDDDLLInterfaces.EDDDLLIF.EDDTerminate edf = (EDDDLLInterfaces.EDDDLLIF.EDDTerminate)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(EDDDLLInterfaces.EDDDLLIF.EDDTerminate));
                    edf();
                }

                BaseUtils.Win32.UnsafeNativeMethods.FreeLibrary(pDll);
                pDll = IntPtr.Zero;
                Version = null;
                DLLOptions = null;
                return true;
            }

            return false;
        }

        public bool Shown()
        {
            if (AssemblyMainType != null)
            {
                if (AssemblyMainType.GetType().GetMethod("EDDMainFormShown") != null)
                {
                    AssemblyMainType.EDDMainFormShown();
                    return true;
                }
            }
            else if (pDll != IntPtr.Zero && pShown != IntPtr.Zero )
            {
                EDDDLLInterfaces.EDDDLLIF.EDDMainFormShown edf = (EDDDLLInterfaces.EDDDLLIF.EDDMainFormShown)Marshal.GetDelegateForFunctionPointer(
                                                                                    pShown,
                                                                                    typeof(EDDDLLInterfaces.EDDDLLIF.EDDMainFormShown));
                edf();
                return true;
            }

            return false;
        }

        public bool Refresh(string cmdr, EDDDLLInterfaces.EDDDLLIF.JournalEntry je)
        {
            if (AssemblyMainType != null)
            {
                if (AssemblyMainType.GetType().GetMethod("EDDRefresh") != null)
                {
                    AssemblyMainType.EDDRefresh(cmdr, je);
                    return true;
                }
            }
            else if (pDll != IntPtr.Zero)
            {
                IntPtr pAddressOfFunctionToCall = BaseUtils.Win32.UnsafeNativeMethods.GetProcAddress(pDll, "EDDRefresh");

                if (pAddressOfFunctionToCall != IntPtr.Zero)
                {
                    EDDDLLInterfaces.EDDDLLIF.EDDRefresh edf = (EDDDLLInterfaces.EDDDLLIF.EDDRefresh)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(EDDDLLInterfaces.EDDDLLIF.EDDRefresh));
                    edf(cmdr, je);
                    return true;
                }
            }

            return false;
        }

        public bool NewJournalEntry(EDDDLLInterfaces.EDDDLLIF.JournalEntry nje, bool stored)
        {
            if (stored && DLLOptions.ContainsIn(EDDDLLInterfaces.EDDDLLIF.FLAG_PLAYLASTFILELOAD) < 0)
                return false;

            if (AssemblyMainType != null)
            {
                if (AssemblyMainType.GetType().GetMethod("EDDNewJournalEntry") != null)
                {
                    AssemblyMainType.EDDNewJournalEntry(nje);
                    return true;
                }
            }
            else if (pDll != IntPtr.Zero && pNewJournalEntry != IntPtr.Zero)
            {
                EDDDLLInterfaces.EDDDLLIF.EDDNewJournalEntry edf = (EDDDLLInterfaces.EDDDLLIF.EDDNewJournalEntry)Marshal.GetDelegateForFunctionPointer(
                                                                                    pNewJournalEntry,
                                                                                    typeof(EDDDLLInterfaces.EDDDLLIF.EDDNewJournalEntry));
                edf(nje);
                return true;
            }

            return false;
        }

        // ACTION DLLCALL <dllname>, "JournalEntry", JID

        public bool ActionJournalEntry(EDDDLLInterfaces.EDDDLLIF.JournalEntry je)
        {
            if (AssemblyMainType != null)
            {
                if (AssemblyMainType.GetType().GetMethod("EDDActionJournalEntry") != null)
                {
                    AssemblyMainType.EDDActionJournalEntry(je);
                    return true;
                }
            }
            else if (pDll != IntPtr.Zero && pActionJournalEntry != IntPtr.Zero)
            {
                EDDDLLInterfaces.EDDDLLIF.EDDActionJournalEntry edf = (EDDDLLInterfaces.EDDDLLIF.EDDActionJournalEntry)Marshal.GetDelegateForFunctionPointer(
                                                                                    pActionJournalEntry,
                                                                                    typeof(EDDDLLInterfaces.EDDDLLIF.EDDActionJournalEntry));
                edf(je);
                return true;
            }

            return false;
        }

        // ACTION DLLCALL <dllname>, cmd, paras..

        public string ActionCommand(string cmd, string[] paras) // paras must be present..
        {
            if (AssemblyMainType != null)
            {
                if (AssemblyMainType.GetType().GetMethod("EDDActionCommand") != null)
                {
                    return AssemblyMainType.EDDActionCommand(cmd, paras);
                }
            }
            else if (pDll != IntPtr.Zero && pActionCommand != IntPtr.Zero)
            {
                EDDDLLInterfaces.EDDDLLIF.EDDActionCommand edf = (EDDDLLInterfaces.EDDDLLIF.EDDActionCommand)Marshal.GetDelegateForFunctionPointer(
                                                                                    pActionCommand,
                                                                                    typeof(EDDDLLInterfaces.EDDDLLIF.EDDActionCommand));
                return edf(cmd, paras);
            }

            return null;
        }

        // Config..

        public bool NewUIEvent(string json)
        {
            if (AssemblyMainType != null)
            {
                if (AssemblyMainType.GetType().GetMethod("EDDNewUIEvent") != null)
                {
                    AssemblyMainType.EDDNewUIEvent(json);
                    return true;
                }
            }
            else if (pDll != IntPtr.Zero && pNewUIEvent != IntPtr.Zero)
            {
                EDDDLLInterfaces.EDDDLLIF.EDDNewUIEvent newui = (EDDDLLInterfaces.EDDDLLIF.EDDNewUIEvent)Marshal.GetDelegateForFunctionPointer(
                                                                                    pNewUIEvent,
                                                                                    typeof(EDDDLLInterfaces.EDDDLLIF.EDDNewUIEvent));
                newui(json);
                return true;
            }

            return false;
        }

        public bool HasConfig()
        {
            if (AssemblyMainType != null)
            {
                return AssemblyMainType.GetType().GetMethod("EDDConfig") != null;
            }
            else
                return pDll != IntPtr.Zero && pConfig != IntPtr.Zero;
        }

        public string Config(string istr, bool editit)
        {
            if (AssemblyMainType != null)
            {
                if (AssemblyMainType.GetType().GetMethod("EDDConfig") != null)
                {
                    return AssemblyMainType.EDDConfig(istr, editit);
                }
            }
            else if (pDll != IntPtr.Zero && pConfig != IntPtr.Zero)
            {
                EDDDLLInterfaces.EDDDLLIF.EDDConfig cfg = (EDDDLLInterfaces.EDDDLLIF.EDDConfig)Marshal.GetDelegateForFunctionPointer(
                                                                                    pConfig,
                                                                                    typeof(EDDDLLInterfaces.EDDDLLIF.EDDConfig));
                return cfg(istr, editit);
            }

            return null;
        }

        public bool NewUnfilteredJournalEntry(EDDDLLInterfaces.EDDDLLIF.JournalEntry nje)
        {
            if (AssemblyMainType != null)
            {
                if (AssemblyMainType.GetType().GetMethod("EDDNewUnfilteredJournalEntry") != null)
                {
                    AssemblyMainType.EDDNewUnfilteredJournalEntry(nje);
                    return true;
                }
            }
            else if (pDll != IntPtr.Zero && pNewUnfilteredJournalEntry != IntPtr.Zero)
            {
                EDDDLLInterfaces.EDDDLLIF.EDDNewUnfilteredJournalEntry edf = (EDDDLLInterfaces.EDDDLLIF.EDDNewUnfilteredJournalEntry)Marshal.GetDelegateForFunctionPointer(
                                                                                    pNewUnfilteredJournalEntry,
                                                                                    typeof(EDDDLLInterfaces.EDDDLLIF.EDDNewUnfilteredJournalEntry));
                edf(nje);
                return true;
            }

            return false;
        }



    }
}
