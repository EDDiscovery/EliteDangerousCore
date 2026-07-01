/*
 * Copyright © 2022-2022 EDDiscovery development team
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

namespace EliteDangerousCore
{
    public static class EliteReleaseDates
    {
        public static DateTime GammaStart {get; } = new DateTime(2014, 11, 22, 4, 0, 0, DateTimeKind.Utc);
        public static DateTime GameRelease {get; } =  new DateTime(2014, 12, 14, 12, 0, 0, DateTimeKind.Utc);
        public static DateTime EDSMRelease {get; } =  new DateTime(2015, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        public static DateTime Release_2_2 {get; } =  new DateTime(2017, 4, 11, 12, 0, 0, DateTimeKind.Utc);
        public static DateTime Release_3_2 {get; } =  new DateTime(2018, 8, 28, 10, 0, 0, DateTimeKind.Utc);
        public static DateTime Release_3_3 {get; } =  new DateTime(2018, 12, 11, 9, 0, 0, DateTimeKind.Utc);
        public static DateTime Odyssey5 {get; } =  new DateTime(2021, 7, 1,12,0,0, DateTimeKind.Utc);
        public static DateTime Odyssey14 {get; } =  new DateTime(2022, 11, 29, 12, 0, 0, DateTimeKind.Utc);          //Galaxy split Live/Legacy
        public static DateTime OdysseyType8 {get; } =  new DateTime(2024, 8, 7, 12, 0, 0, DateTimeKind.Utc);
        public static DateTime Ascendency {get; } =  new DateTime(2024, 10, 22, 12, 0, 0, DateTimeKind.Utc);         // power play 2.0
        public static DateTime Trailblazers {get; } =  new DateTime(2025, 2, 26, 12, 0, 0, DateTimeKind.Utc);        // colonisation
        public static DateTime Vanguards {get; } =  new DateTime(2025, 8, 19, 12, 0, 0,DateTimeKind.Utc);            //squadron overhaul
        public static DateTime GameEndTime {get; } =  new DateTime(2999, 12, 14, 23, 59, 59, DateTimeKind.Utc);      // not according to the forums, its already dead!

        public static bool IsBeta(string GameVersion, string Build, DateTime EventTimeUTC)
        {
            if (GameVersion.Contains("Beta", StringComparison.InvariantCultureIgnoreCase) ||
                    GameVersion.Contains("Gamma", StringComparison.InvariantCultureIgnoreCase) ||
                    GameVersion.Contains("Alpha", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (GameVersion.Contains("April Update EDH") && (Build.Contains("r198057/r0") || Build.Contains("r197746/r0")))
                return true;

            if (GameVersion.Equals("2.2") && (Build.Contains("r121645/r0") || Build.Contains("r129516/r0")))
                return true;

            if (Build.Contains("r304032/r0") && EventTimeUTC < EliteReleaseDates.OdysseyType8) // august 2024 pre-release for T8
                return true;

            if (GameVersion.Equals("4.0.0.1903") && (Build.Contains("r308286/r0")))
                return true;

            if (GameVersion.Equals("4.1.3.0") && (Build.Contains("r316037/r0")))
                return true;

            if (GameVersion.Equals("4.2.1.0") && (Build.Contains("r319022/r0")))
                return true;

            if (GameVersion.Equals("4.3.0.0") && (Build.Contains("r321601/r0")))
                return true;

            if (GameVersion.Equals("4.3.1.0") && (Build.Contains("r324270/r0")))
                return true;

            if (GameVersion.Equals("4.3.3.0") && (Build.Contains("r327080/r0")))
                return true;

            if (GameVersion.Equals("4.4.0.0") && (Build.Contains("r329880/r0") || Build.Contains("STUPID FRONTIER REUSING THIS ID FOR RELEASE r330116/r0")))        // beta for nomad June 26
                return true;

            return false;
        }

    }
    public static class EliteFixesDates
    {
        public static DateTime ED_No_Training_Timestamp {get; } =  new DateTime(2017, 10, 4, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime ED_No_Faction_Timestamp {get; } =  new DateTime(2017, 9, 26, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime TotalEarningsCorrectDate {get; } =  new DateTime(2018, 5, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    public static class EDDFixesDates
    {
        public static DateTime EDSMMinimumSystemsDate {get; } =  new DateTime(2015, 5, 10, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime BookmarkUTCswitchover {get; } =  new DateTime(2020, 1, 23, 0, 0, 0, DateTimeKind.Utc);
    }
}
