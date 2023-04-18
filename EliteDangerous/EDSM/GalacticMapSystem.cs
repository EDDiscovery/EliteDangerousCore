/*
 * Copyright © 2016-2021 EDDiscovery development team
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

using System.Text.RegularExpressions;

namespace EliteDangerousCore.EDSM
{
    public class GalacticMapSystem : SystemClass
    {
        public GalacticMapObject GalMapObject { get; set; }

        public GalacticMapSystem(ISystem sys, GalacticMapObject gmo) : base(sys)
        {
            this.GalMapObject = gmo;
        }

        public GalacticMapSystem(GalacticMapObject gmo) : base()
        {
            this.Name = gmo.GalMapSearch;
            this.X = gmo.Points[0].X;
            this.Y = gmo.Points[0].Y;
            this.Z = gmo.Points[0].Z;
            this.GalMapObject = gmo;
        }
    }
}
