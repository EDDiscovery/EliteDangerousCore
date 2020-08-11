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

using System.Text.RegularExpressions;

namespace EliteDangerousCore.EDSM
{
    public class GalacticMapSystem : SystemClass
    {
        private Regex EDSMIdRegex = new Regex("/system/id/([0-9]+)/");

        public GalacticMapObject GalMapObject { get; set; }

        public GalacticMapSystem(ISystem sys, GalacticMapObject gmo) : base(sys)
        {
            this.GalMapObject = gmo;
        }

        public GalacticMapSystem(GalacticMapObject gmo) : base()
        {
            this.Name = gmo.galMapSearch;
            this.X = gmo.points[0].X;
            this.Y = gmo.points[0].Y;
            this.Z = gmo.points[0].Z;
            this.GalMapObject = gmo;

            if (gmo.galMapUrl != null)
            {
                var rematch = EDSMIdRegex.Match(gmo.galMapUrl);

                long edsmid;
                if (rematch != null && long.TryParse(rematch.Groups[1].Value, out edsmid))
                {
                    this.EDSMID = edsmid;
                }
            }
        }
    }
}
