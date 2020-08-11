/*
 * Copyright © 2015 - 2019 EDDiscovery development team
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

namespace EliteDangerousCore
{
    public class EliteDangerousTypesFromJSON
    {
        static public EDGovernment Government2ID(string str)
        {
            foreach (var govid in Enum.GetValues(typeof(EDGovernment)))
            {
                if (str.Equals(govid.ToString().Replace("_", " ")))
                    return (EDGovernment)govid;
                //System.Console.WriteLine(govid.ToString());
            }

            return EDGovernment.Unknown;
        }

        static public EDAllegiance Allegiance2ID(string str)
        {
            foreach (var govid in Enum.GetValues(typeof(EDAllegiance)))
            {
                if (str.Equals(govid.ToString().Replace("_", " ")))
                    return (EDAllegiance)govid;
                //System.Console.WriteLine(govid.ToString());
            }

            return EDAllegiance.Unknown;
        }


        static public EDState EDState2ID(string str)
        {
            foreach (var govid in Enum.GetValues(typeof(EDState)))
            {
                if (str.Equals(govid.ToString().Replace("_", " ")))
                    return (EDState)govid;
                //System.Console.WriteLine(govid.ToString());
            }

            return EDState.Unknown;
        }


        static public EDSecurity EDSecurity2ID(string str)
        {
            foreach (var govid in Enum.GetValues(typeof(EDSecurity)))
            {
                if (str.Equals(govid.ToString().Replace("_", " ")))
                    return (EDSecurity)govid;
                //System.Console.WriteLine(govid.ToString());
            }

            return EDSecurity.Unknown;
        }

        static public EDEconomy EDEconomy2ID(string str)
        {
            foreach (var govid in Enum.GetValues(typeof(EDEconomy)))
            {
                if (str.Equals(govid.ToString().Replace("___", ".").Replace("__", "-").Replace("_", " ")))
                    return (EDEconomy)govid;
                //System.Console.WriteLine(govid.ToString());
            }

            return EDEconomy.Unknown;
        }
    }
}
