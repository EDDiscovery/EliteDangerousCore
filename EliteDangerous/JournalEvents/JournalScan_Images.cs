/*
 * Copyright © 2016 - 2023 EDDiscovery development team
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

using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore.JournalEvents
{
    public partial class JournalScan : JournalEntry
    {
        public string GetStarPlanetTypeImageName()
        {
            return IsStar ? StarTypeImageName : PlanetClassImageName;
        }

        [PropertyNameAttribute(null)] // cancel

        public string StarTypeImageName                      // property so its gets outputted via JSON
        {
            get
            {
                if (!IsStar)
                {
                    return $"Bodies.Unknown";
                }

                return BodyToImages.StarTypeImageName(StarTypeID, nStellarMass, nSurfaceTemperature);
            }
        }

        [PropertyNameAttribute(null)]       // cancel
        public string PlanetClassImageName       // property so its gets outputted via JSON
        {
            get
            {
                if (!IsPlanet)
                {
                    return $"Bodies.Unknown";
                }

                return BodyToImages.PlanetClassImageName(PlanetTypeID, nSurfaceTemperature, AtmosphereComposition, AtmosphereProperty, AtmosphereID,
                                                         Terraformable, nLandable, nMassEM, nTidalLock);
            }
        }
    }
}


