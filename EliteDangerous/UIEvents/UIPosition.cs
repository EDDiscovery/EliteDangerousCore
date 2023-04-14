/*
 * Copyright © 2016 - 2021 EDDiscovery development team
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
 *
 */
using System;

namespace EliteDangerousCore.UIEvents
{
    // NOTE you get this when position appears AND disappears, check for validity before use

    public class UIPosition : UIEvent
    {
        public UIPosition(DateTime time, bool refresh) : base(UITypeEnum.Position, time, refresh)
        {
            Location = new Position();
        }
        public UIPosition() : base(UITypeEnum.Position, DateTime.UtcNow, false)
        {
            Location = new Position();
        }

        public UIPosition(Position value, double head, double planetradius, string bodyname, DateTime time, bool refresh) : this(time, refresh)
        {
            Location = value;
            Heading = head;
            PlanetRadius = planetradius;
            BodyName = bodyname;
        }


        public override string ToString()
        {
            string s = "Invalid";
            if (Location.ValidPosition)
            {
                s = $"{Location.Latitude} {Location.Longitude}";
                if (Location.ValidAltitude)
                    s += $" {Location.Altitude}m";
                if (ValidHeading)
                    s += $" {Heading} deg";
            }

            return s;
        }

        public Position Location { get; private set; }

        public const double InvalidValue = -999999;    // change to make it more JSON friendly, must be synchronised with EDStatusReader::InvalidValue

        // you MAY not get heading.

        public double Heading { get; private set; } = InvalidValue;
        public bool ValidHeading { get { return Heading != InvalidValue; } }

        public double PlanetRadius { get; private set; } = InvalidValue;
        public bool ValidRadius { get { return PlanetRadius != InvalidValue; } }

        public string BodyName { get; private set; } = "";  // full body name, incl system
        public bool ValidBodyName { get { return BodyName.HasChars(); } }

        public class Position
        {
            public Position()
            {
                Latitude = Longitude = Altitude = InvalidValue;
                AltitudeFromAverageRadius = false;
            }

            // you MAY get position without Altitude.. seen in SRV it doing that.  Code defensively

            public bool ValidPosition { get { return Latitude != InvalidValue && Latitude != InvalidValue; } }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public bool ValidAltitude { get { return Altitude != InvalidValue; } }
            public double Altitude { get; set; }
            public bool AltitudeFromAverageRadius { get; set; }
        }
    }
}
