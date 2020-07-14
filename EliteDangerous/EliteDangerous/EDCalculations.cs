using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCore
{
    public static class EliteDangerousCalculations
    {
        //Based on http://elite-dangerous.wikia.com/wiki/Frame_Shift_Drive

        public class FSDSpec
        {
            public int FsdClass { get; set; }
            public string Rating { get; set; }
            public double PowerConstant { get; set; }
            public double LinearConstant { get; set; }
            public double OptimalMass { get; set; }
            public double MaxFuelPerJump { get; set; }
            public double FSDBoosterRange { get; set; }

            public FSDSpec(int fsdClass,
                string rating,
                double pc,
                double lc,
                double mOpt,
                double mfpj)
            {
                this.FsdClass = fsdClass;
                this.Rating = rating;
                this.PowerConstant = pc;
                this.LinearConstant = lc;
                this.OptimalMass = mOpt;
                this.MaxFuelPerJump = mfpj;
                this.FSDBoosterRange = 0;
            }

            public void SetFSDBooster(int fsdBoosterClass)
            {
                if (fsdBoosterClass > 0 && fsdBoosterClass <= 5 )
                {
                    FSDBoosterRange = FSDBoosterSpec[fsdBoosterClass];
                }
                else
                {
                    FSDBoosterRange = 0;
                }
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

            public JumpInfo GetJumpInfo(int cargo, double mass, double currentfuel,double avgfuel)
            {
                JumpInfo jid = new JumpInfo();
                jid.cursinglejump = MaxJump(cargo, mass, currentfuel);
                jid.curfumessinglejump = MaxJump(cargo, mass, MaxFuelPerJump);
                jid.unladenmaxsinglejump = MaxJump(0, mass, MaxFuelPerJump);
                jid.avgsinglejump = MaxJump(cargo, mass, avgfuel);
                jid.avgsinglejumpnocargo = MaxJump(0, mass, avgfuel);
                jid.maxjumprange = CalculateMaxJumpDistance(cargo, mass, currentfuel, out jid.maxjumps);
                return jid;
            }

            public double MaxJump(int currentCargo, double unladenMass, double fuel)
            {
                return Math.Pow(MaxFuelPerJump / (LinearConstant * 0.001), 1 / PowerConstant) * OptimalMass / (currentCargo + unladenMass + fuel) + FSDBoosterRange;
            }

            public double CalculateMaxJumpDistance(double cargo, double unladenmass, double fuel, out double jumps)
            {
                double fr = fuel % MaxFuelPerJump;                // fraction of fuel left.. up to maximum of fuel per jump

                jumps = Math.Floor(fuel / MaxFuelPerJump);        // number of jumps possible PAST first one (Floor)

                double mass = unladenmass + fr + cargo;  // weight with just fuel on board for 1 jump

                double d = 0.0;

                if (fuel > 0.0)
                    d = Math.Pow(fr / (LinearConstant * 0.001), 1 / PowerConstant) * OptimalMass / mass + FSDBoosterRange;      // fr is what we have for 1 jump... This is probably incorrect for the boost but it is the same formula as coriolis

                for (int idx = 0; idx < jumps; idx++)   // if any more jumps past the first
                {
                    mass += MaxFuelPerJump;
                    d += Math.Pow(MaxFuelPerJump / (LinearConstant * 0.001), 1 / PowerConstant) * OptimalMass / mass + FSDBoosterRange;
                }

                return d;
            }

            public double FuelUse(double cargo, double unladenmass, double distance)  
            {
                double mass = unladenmass + cargo;  // weight with just fuel on board for 1 jump

                return LinearConstant * 0.001 * Math.Pow((distance * mass / OptimalMass), PowerConstant);
            }

            public override string ToString()
            {
                return "Rating: " + Rating + Environment.NewLine +
                       "Class: " + FsdClass + Environment.NewLine +
                       "Power Constant: " + PowerConstant + Environment.NewLine +
                       "Linear Constant: " + LinearConstant + Environment.NewLine +
                       "Optimum Mass: " + OptimalMass + "t" + Environment.NewLine +
                       "Max Fuel Per Jump: " + MaxFuelPerJump + "t";
            }
        }

        public static FSDSpec FindFSD(int cls, string rat )     // allow rat to be null.  May be null return if not found
        {
            return rat != null ? FSDList.Find(x => x.FsdClass == cls && x.Rating.Equals(rat, StringComparison.InvariantCultureIgnoreCase)) : null;
        }

        public static List<FSDSpec> FSDList = new List<FSDSpec>(        // verified against website on 5 april 2018..  
            new FSDSpec[] {                                             // looks same as coriolis 
                          //C    R      pc  lc  mop mfpj
                new FSDSpec(2,  "E",    2,  11, 48, 0.6),                // coriolis-data: frame_shift_drive.json   2A: fuelmul = 0.012, fuelpower=2
                new FSDSpec(2,  "D" ,   2,  10, 54, 0.6),                // APP/shipyard/calculations.js does the same function
                new FSDSpec(2,  "C" ,   2,  8,  60, 0.6),
                new FSDSpec(2,  "B" ,   2,  10, 75, 0.8),
                new FSDSpec(2,  "A",    2,  12, 90, 0.9),

                new FSDSpec(3,  "E",    2.15    ,11 ,80,    1.2),
                new FSDSpec(3,  "D",    2.15    ,10 ,90,    1.2),
                new FSDSpec(3,  "C" ,   2.15,   8   ,100,   1.2),
                new FSDSpec(3,  "B",    2.15,   10  ,125    ,1.5),
                new FSDSpec(3,  "A" ,   2.15    ,12 ,150    ,1.8),

                new FSDSpec(4,  "E",    2.3,    11  ,280,   2),
                new FSDSpec(4,  "D" ,   2.3,    10, 315 ,2),
                new FSDSpec(4,  "C"     ,2.3    ,8  ,350,   2),
                new FSDSpec(4,  "B" ,   2.3 ,10,    438,    2.5),
                new FSDSpec(4   ,"A",   2.3,    12  ,525    ,3),

                new FSDSpec(5,  "E" ,   2.45,   11, 560 ,3.3),
                new FSDSpec(5,  "D" ,   2.45    ,10 ,630,   3.3),
                new FSDSpec(5   ,"C"    ,2.45,  8   ,700    ,3.3),
                new FSDSpec(5   ,"B"    ,2.45   ,10 ,875    ,4.1),
                new FSDSpec(5   ,"A"    ,2.45   ,12 ,1050   ,5),

                new FSDSpec(6   ,"E"    ,2.6    ,11 ,960    ,5.3),
                new FSDSpec(6   ,"D"    ,2.6    ,10 ,1080   ,5.3),
                new FSDSpec(6   ,"C"    ,2.6    ,8  ,1200   ,5.3),
                new FSDSpec(6   ,"B"    ,2.6    ,10 ,1500   ,6.6),
                new FSDSpec(6   ,"A"    ,2.6    ,12 ,1800   ,8),

                new FSDSpec(7,  "E"     ,2.75   ,11 ,1440   ,8.5),
                new FSDSpec(7   ,"D"    ,2.75   ,10 ,1620   ,8.5),
                new FSDSpec(7   ,"C"    ,2.75   ,8, 1800    ,8.5),
                new FSDSpec(7   ,"B"    ,2.75   ,10 ,2250   ,10.6),
                new FSDSpec(7   ,"A"    ,2.75   ,12 ,2700   ,12.8)
            }
        );

        public static double[] FSDBoosterSpec = { 0, 4, 6, 7.75, 9.25, 10.5 }; // Boost range with module size as index
    }
}
