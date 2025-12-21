/*
 * Copyright © 2022-2024 EDDiscovery development team
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

using System;
using System.Collections.Generic;

namespace EliteDangerousCore
{
    public static class EliteDangerousCalculations
    {
        //Based on http://elite-dangerous.wikia.com/wiki/Frame_Shift_Drive

        public class FSDSpec
        {
            public double PowerConstant { get; set; }
            public double LinearConstant { get; set; }
            public double OptimalMass { get; set; }
            public double MaxFuelPerJump { get; set; }
            public double FSDGuardianBoosterRange { get; set; }     // set if you have a guardian booster (ly)
            public double FuelMultiplier { get { return LinearConstant * 0.001; } }
            public double PowerFactor { get { return Math.Pow(MaxFuelPerJump / FuelMultiplier, 1 / PowerConstant); } }
            public int NeutronMultipler { get; set; }          // default 4 for multiplier, 6 for caspian

            public FSDSpec(
                double pc,
                double lc,
                double mOpt,
                double mfpj,
                int nmult)
            {
                this.PowerConstant = pc;
                this.LinearConstant = lc;
                this.OptimalMass = mOpt;
                this.MaxFuelPerJump = mfpj;
                this.FSDGuardianBoosterRange = 0;
                this.NeutronMultipler = nmult;
            }

            public double spanshboostedFuelMultiplier(double mass)
            {
                double fuelMultiplier = (LinearConstant * 0.001);
                if (FSDGuardianBoosterRange > 0)
                {
                    double range = (OptimalMass / mass) * (Math.Pow(MaxFuelPerJump / fuelMultiplier, 1 / PowerConstant));
                    fuelMultiplier = fuelMultiplier * Math.Pow(range / (range + FSDGuardianBoosterRange), PowerConstant);
                }
                return fuelMultiplier;
            }

            public double SpanshJumpRange(double mass)
            {
                double fuelMultiplier = spanshboostedFuelMultiplier(mass);
                double massf = OptimalMass / mass;
                double powerf = Math.Pow(MaxFuelPerJump / fuelMultiplier, 1 / PowerConstant);

                return massf * powerf;
            }

            public double JumpRangeOriginal(int currentCargo, double unladenMassHullModules, double fuel, double boost)
            {
                double mass = currentCargo + unladenMassHullModules + fuel;
                double massf = OptimalMass / mass;
                double fuelmultiplier = (LinearConstant * 0.001);
                double powerf = Math.Pow(MaxFuelPerJump / fuelmultiplier, 1 / PowerConstant);
                double basev = powerf * massf;
                return (basev + FSDGuardianBoosterRange) * boost;
            }

            public double JumpRange(int currentCargo, double unladenMassHullModules, double fuel, double boost)
            {
                if (fuel == 0)
                    return 0;
                double mass = currentCargo + unladenMassHullModules + fuel;
                double massf = OptimalMass / mass;
                double fuelmultiplier = (LinearConstant * 0.001);
                double powerf = Math.Pow(Math.Min(fuel, MaxFuelPerJump) / fuelmultiplier, 1 / PowerConstant); //important: Use fuel value instead of MFPJ in case fuel level is below MFPJ
                double basev = powerf * massf;
                return (basev + FSDGuardianBoosterRange) * boost;
            }

            public class JumpInfo
            {
                public double cursinglejump;
                public double curfumessinglejump;
                public double unladenmaxsinglejump;
                public double avgsinglejump;
                public double avgsinglejumpnocargo;
                public double maxjumprange;         // using current fuel amount
                public double maxjumps;
            }

            // boost is multiplier due to neutron (4), jet cone (1.5), or synthesis (1.25/1.5/2)
            public JumpInfo GetJumpInfo(int cargo, double mass, double currentfuel, double avgfuel, double boost)
            {
                JumpInfo jid = new JumpInfo();
                jid.cursinglejump = JumpRange(cargo, mass, currentfuel, boost);
                jid.curfumessinglejump = JumpRange(cargo, mass, MaxFuelPerJump, boost);
                jid.unladenmaxsinglejump = JumpRange(0, mass, MaxFuelPerJump, boost);
                jid.avgsinglejump = JumpRange(cargo, mass, avgfuel, boost);
                jid.avgsinglejumpnocargo = JumpRange(0, mass, avgfuel, boost);
                jid.maxjumprange = CalculateMaxJumpDistance(cargo, mass, currentfuel, out jid.maxjumps);
                return jid;
            }

            public double CalculateMaxJumpDistance(double cargo, double unladenmass, double fuel, out double jumps)
            {
                double fr = fuel % MaxFuelPerJump;                // fraction of fuel left.. up to maximum of fuel per jump

                jumps = Math.Floor(fuel / MaxFuelPerJump);        // number of jumps possible PAST first one (Floor)

                double mass = unladenmass + fr + cargo;  // weight with just fuel on board for 1 jump

                double d = 0.0;

                if (fuel > 0.0)
                    d = Math.Pow(fr / (LinearConstant * 0.001), 1 / PowerConstant) * OptimalMass / mass + FSDGuardianBoosterRange;      // fr is what we have for 1 jump... This is probably incorrect for the boost but it is the same formula as coriolis

                for (int idx = 0; idx < jumps; idx++)   // if any more jumps past the first
                {
                    mass += MaxFuelPerJump;
                    d += Math.Pow(Math.Min(fuel, MaxFuelPerJump) / (LinearConstant * 0.001), 1 / PowerConstant) * OptimalMass / mass + FSDGuardianBoosterRange;
                }

                return d;
            }

            // from EDCD 20/11/22 updated fuel use . Note refill tank is not taken into account in frontiers calc
            public double FuelUse(double cargo, double unladenmass, double fuel, double distance, double boost)
            {
                double mass = unladenmass + cargo + fuel;  // weight

                double basemaxrange = (OptimalMass / mass) * Math.Pow((Math.Min(fuel, MaxFuelPerJump) * 1000 / LinearConstant), (1 / PowerConstant));
                double boostfactor = Math.Pow((basemaxrange / (basemaxrange + FSDGuardianBoosterRange)), PowerConstant);

                return boostfactor * LinearConstant * 0.001 * Math.Pow(((distance / boost) * mass / OptimalMass), PowerConstant);
            }

            public override string ToString()
            {
                return "Power Constant: " + PowerConstant + Environment.NewLine +
                       "Linear Constant: " + LinearConstant + Environment.NewLine +
                       "Optimum Mass: " + OptimalMass + "t" + Environment.NewLine +
                       "Max Fuel Per Jump: " + MaxFuelPerJump + "t";
            }
        }
    }
}


