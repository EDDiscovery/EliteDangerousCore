/*
 * Copyright 2016 - 2023 EDDiscovery development team
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

using QuickJSON;
using System;
using System.Collections.Generic;
using System.Text;
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore.JournalEvents
{
    public partial class JournalScan
    {
        public HabZones GetHabZones()
        {
            if (IsStar && nRadius.HasValue && nSurfaceTemperature.HasValue)
            {
                HabZones hz = new HabZones(nRadius.Value, nSurfaceTemperature.Value);
                // values initially calculated by Jackie Silver (https://forums.frontier.co.uk/member.php/37962-Jackie-Silver)
                return hz;
            }
            else
                return null;
        }


        #region Other Queries

        public StarPlanetRing FindRing(string name)
        {
            if (Rings != null)
                return Array.Find(Rings, x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            else
                return null;
        }

        public double GetMaterial(string v)
        {
            return Materials == null || !Materials.ContainsKey(v.ToLowerInvariant()) ? 0.0 : Materials[v.ToLowerInvariant()];
        }

        public double? GetAtmosphereComponent(string c)
        {
            if (!HasAtmosphericComposition)
                return null;
            if (!AtmosphereComposition.ContainsKey(c))
                return 0.0;
            return AtmosphereComposition[c];

        }

        public double? GetCompositionPercent(string c)
        {
            if (!HasPlanetaryComposition)
                return null;
            if (!PlanetComposition.ContainsKey(c))
                return 0.0;
            return PlanetComposition[c];
        }

        private ScanEstimatedValues EstimatedValues = null;

        public ScanEstimatedValues GetEstimatedValues()         // get, compute if never computed before. It will never change after calc.
        {
            if (EstimatedValues == null)
            {
                EstimatedValues = new ScanEstimatedValues(EventTimeUTC, IsStar, StarTypeID, IsPlanet, PlanetTypeID, Terraformable, nStellarMass, nMassEM, IsOdysseyEstimatedValues());
            }

            return EstimatedValues;
        }

        [JsonIgnore]
        public bool PR31State { get { return IsNotPreviouslyDiscovered && IsPreviouslyMapped; } }         // condition of bodies in the bubble, marked not discovered, but mapped

        // work out what is possible now
        // previously in EstimatedValues
        // showimpossiblevalues turns on reporting of values which cannot now be achieved
        // values not possible are -1
        public void GetPossibleEstimatedValues(bool showimpossiblevalues,
                                            out long basevalue,
                                            out long mappedvalue, out long mappedefficiently,                       // previous mapped and not mapped
                                            out long firstmappedvalue, out long firstmappedefficiently,             // not previous mapped and not mapped
                                            out long firstdiscoveredmappedvalue, out long firstdiscoveredmappedefficiently, // not previously discovred or mapped and not mapped
                                            out long best // best value now possible
                                            )
        {
            var ev = GetEstimatedValues();

            basevalue = ev.EstimatedValueBase;
            best = IsPreviouslyDiscovered ? basevalue : ev.EstimatedValueFirstDiscovered;       // if previously discovered, best base is basevalue, else its first discovered tag

            mappedvalue = mappedefficiently = -1;
            firstmappedvalue = firstmappedefficiently = -1;
            firstdiscoveredmappedvalue = firstdiscoveredmappedefficiently = -1;

            if (ev.EstimatedValueMapped > 0)        // if value is set by estimator (not on stars)
            {
                // work out if previous mapped but you've not mapped yet
                bool notpreviouslymappedandnotmapped = IsPreviouslyMapped && Mapped == false;

                if (showimpossiblevalues || notpreviouslymappedandnotmapped)
                {
                    mappedefficiently = ev.EstimatedValueMappedEfficiently;
                    mappedvalue = ev.EstimatedValueMapped;
                }

                best = ev.EstimatedValueMappedEfficiently;
            }


            if (ev.EstimatedValueFirstMappedEfficiently > 0)
            {
                // Note EDSM bodies are marked as wasdiscovered=true, wasmapped=false (don't know so presume not)

                // First Mapped: shown if previously discovered, not previously mapped and we have not mapped
                bool firstmappossible = IsPreviouslyDiscovered && IsNotPreviouslyMapped && Mapped == false;

                if (showimpossiblevalues || firstmappossible)
                {
                    firstmappedefficiently = ev.EstimatedValueFirstMappedEfficiently;
                    firstmappedvalue = ev.EstimatedValueFirstMapped;
                }

                if (!IsPreviouslyMapped)            // if we can acheive a first map, thats good
                    best = ev.EstimatedValueFirstMappedEfficiently;
            }


            if (ev.EstimatedValueFirstDiscoveredFirstMappedEfficiently > 0)
            {
                // First Discovered Mapped: shown if not in pr31, not discovered, not mapped, have not mapped
                bool firstdiscoveredmappossible = !PR31State && IsNotPreviouslyDiscovered && IsNotPreviouslyMapped && Mapped == false;

                if (showimpossiblevalues || firstdiscoveredmappossible)
                {
                    firstdiscoveredmappedefficiently = ev.EstimatedValueFirstDiscoveredFirstMappedEfficiently;
                    firstdiscoveredmappedvalue = ev.EstimatedValueFirstDiscoveredFirstMapped;
                }
                
                if (!IsPreviouslyDiscovered && !IsPreviouslyMapped)     // if we can acheive a first discover and first map, good
                    best = ev.EstimatedValueFirstDiscoveredFirstMappedEfficiently;
            }
        }

        public void AccumulateJumponium(ref string jumponium)
        {
            if (IsLandable == true && HasMaterials) // Landable bodies with valuable materials, collect into jumponimum
            {
                int basic = 0;
                int standard = 0;
                int premium = 0;

                foreach (KeyValuePair<string, double> mat in Materials)
                {
                    string usedin = Recipes.UsedInSythesisByFDName(mat.Key);
                    if (usedin.Contains("FSD-Basic"))
                        basic++;
                    if (usedin.Contains("FSD-Standard"))
                        standard++;
                    if (usedin.Contains("FSD-Premium"))
                        premium++;
                }

                if (basic > 0 || standard > 0 || premium > 0)
                {
                    int mats = basic + standard + premium;

                    StringBuilder jumpLevel = new StringBuilder();

                    if (basic != 0)
                        jumpLevel.AppendPrePad(basic + "/" + Recipes.FindSynthesis("FSD", "Basic").Count + " Basic".Tx(), ", ");
                    if (standard != 0)
                        jumpLevel.AppendPrePad(standard + "/" + Recipes.FindSynthesis("FSD", "Standard").Count + " Standard".Tx(), ", ");
                    if (premium != 0)
                        jumpLevel.AppendPrePad(premium + "/" + Recipes.FindSynthesis("FSD", "Premium").Count + " Premium".Tx(), ", ");

                    jumponium = jumponium.AppendPrePad(string.Format("{0} has {1} level elements.".Tx(), BodyName, jumpLevel), Environment.NewLine);
                }
            }
        }


        // adds to mats hash (if required) if one found.
        // returns number of jumponiums in body
        public int Jumponium(HashSet<string> mats = null)
        {
            int count = 0;

            foreach (var m in Materials.EmptyIfNull())
            {
                string n = m.Key.ToLowerInvariant();
                if (MaterialCommodityMicroResourceType.IsJumponiumType(n))
                {
                    count++;
                    if (mats != null && !mats.Contains(n))      // and we have not counted it
                    {
                        mats.Add(n);
                    }
                }
            }

            return count;
        }

        #endregion

    }
}


