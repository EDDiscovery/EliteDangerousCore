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
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using QuickJSON;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.ScanBaryCentre)]
    public class JournalScanBaryCentre : JournalEntry, IStarScan
    {
        public JournalScanBaryCentre(JObject evt) : base(evt, JournalTypeEnum.ScanBaryCentre)
        {
            StarSystem = evt["StarSystem"].Str();
            SystemAddress = evt["SystemAddress"].Long();
            BodyID = evt["BodyID"].Int();                               // ALL
            SemiMajorAxis = evt["SemiMajorAxis"].Double();   
            Eccentricity = evt["Eccentricity"].Double();
            OrbitalInclination = evt["OrbitalInclination"].Double();
            Periapsis = evt["Periapsis"].Double();
            OrbitalPeriod = evt["OrbitalPeriod"].Double();
            AscendingNode = evt["AscendingNode"].Double();
            MeanAnomaly = evt["MeanAnomaly"].Double();

        }

        public string StarSystem { get; private set; }                    
        public long SystemAddress { get; private set; }                   
        public int BodyID { get; private set; }                            

        public double SemiMajorAxis { get; private set; }
        public double SemiMajorAxisAU { get { return SemiMajorAxis / BodyPhysicalConstants.oneAU_m; } }
        public string SemiMajorAxisLSKM { get { return (SemiMajorAxis >= BodyPhysicalConstants.oneLS_m / 10 ? ((SemiMajorAxis / BodyPhysicalConstants.oneLS_m).ToString("N1") + "ls") : ((SemiMajorAxis / 1000).ToString("N0") + "km")); } }

        public double Eccentricity { get; private set; }                  
        public double OrbitalInclination { get; private set; }            
        public double Periapsis { get; private set; }
        public double OrbitalPeriod { get; private set; }
        public double OrbitalPeriodDays { get { return OrbitalPeriod / BodyPhysicalConstants.oneDay_s; } }

        public double AscendingNode { get; private set; }
        public double MeanAnomaly { get; private set; }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddBaryCentre(this,system);
        }

        public override void FillInformation(ISystem sysunused, string whereamiunused, out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("SMA:", SemiMajorAxisLSKM, "Period:;d;0.0", OrbitalPeriodDays);
            detailed = BaseUtils.FieldBuilder.Build("e:;;0.000", Eccentricity, "OI:;;0.000", OrbitalInclination,
                                                "AN:;;0.000", AscendingNode, "MA:;;0.000", MeanAnomaly);
        }
    }

}



