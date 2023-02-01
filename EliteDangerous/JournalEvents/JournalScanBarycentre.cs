/*
 * Copyright © 2016-2023 EDDiscovery development team
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

using QuickJSON;
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore.JournalEvents
{
    [System.Diagnostics.DebuggerDisplay("Barycentre {EventTimeUTC} {StarSystem} {BodyID} {SemiMajorAxis}")]
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

        [PropertyNameAttribute("Barycentre system")]
        public string StarSystem { get; private set; }
        [PropertyNameAttribute("Frontier system address")]
        public long SystemAddress { get; private set; }
        [PropertyNameAttribute("Frontier body ID")]
        public int BodyID { get; private set; }

        [PropertyNameAttribute("SMA in m")]
        public double SemiMajorAxis { get; private set; }
        [PropertyNameAttribute("SMA in AU")]
        public double SemiMajorAxisAU { get { return SemiMajorAxis / BodyPhysicalConstants.oneAU_m; } }
        [PropertyNameAttribute("SMA in LS")]
        public double SemiMajorAxisLS { get { return SemiMajorAxis / BodyPhysicalConstants.oneLS_m; } }
        [PropertyNameAttribute("SMA in LS if > 0.1 LS, or km")]
        public string SemiMajorAxisLSKM { get { return (SemiMajorAxis >= BodyPhysicalConstants.oneLS_m / 10 ? ((SemiMajorAxis / BodyPhysicalConstants.oneLS_m).ToString("N1") + "ls") : ((SemiMajorAxis / 1000).ToString("N0") + "km")); } }

        [PropertyNameAttribute("Eccentricity of orbit")]
        public double Eccentricity { get; private set; }
        [PropertyNameAttribute("Degrees")]
        public double OrbitalInclination { get; private set; }
        [PropertyNameAttribute("Degrees")]
        public double Periapsis { get; private set; }
        [PropertyNameAttribute("Seconds")]
        public double OrbitalPeriod { get; private set; }
        [PropertyNameAttribute("Days")]
        public double OrbitalPeriodDays { get { return OrbitalPeriod / BodyPhysicalConstants.oneDay_s; } }

        [PropertyNameAttribute("Degrees")]
        public double AscendingNode { get; private set; }
        [PropertyNameAttribute("Degrees")]
        public double MeanAnomaly { get; private set; }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddBarycentre(this,system);
        }

        public override void FillInformation(out string info, out string detailed)
        {
            info = BaseUtils.FieldBuilder.Build("SMA:", SemiMajorAxisLSKM, "Period:;d;0.0", OrbitalPeriodDays, "ID:", BodyID);
            detailed = BaseUtils.FieldBuilder.Build("e:;;0.000", Eccentricity, "OI:;;0.000", OrbitalInclination,
                                                "AN:;;0.000", AscendingNode, "MA:;;0.000", MeanAnomaly);
        }
    }

}



