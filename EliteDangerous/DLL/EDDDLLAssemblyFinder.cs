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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EliteDangerousCore.DLL
{
    public static class EDDDLLAssemblyFinder
    {
        public static List<string> AssemblyFindPaths { get; set; } = new List<string>();

        public static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            string filename = args.Name.Split(',')[0] + ".dll".ToLowerInvariant();

            foreach (var path in AssemblyFindPaths)
            {
                FileInfo[] find = Directory.EnumerateFiles(path, filename, SearchOption.AllDirectories).Select(f => new FileInfo(f)).OrderBy(p => p.LastWriteTime).ToArray();

                foreach (var f in find)
                {
                    try
                    {
                        AssemblyName currentAssemblyName = AssemblyName.GetAssemblyName(f.FullName);

                        if (args.Name == currentAssemblyName.FullName)      // check full name in case we have multiple ones with different versions
                        {
                            System.Diagnostics.Debug.WriteLine("Resolved " + filename + " from " + find[0].FullName);
                            return System.Reflection.Assembly.LoadFrom(find[0].FullName);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception during load " + ex);
                    }
                }

                System.Diagnostics.Debug.WriteLine("UnResolved " + filename + " from " + find[0].FullName);
            }

            return null;

        }
    }
}
