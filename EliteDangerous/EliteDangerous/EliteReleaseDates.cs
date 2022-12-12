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
        public static DateTime GammaStart = new DateTime(2014, 11, 22, 4, 0, 0, DateTimeKind.Utc);
        public static DateTime Release_2_2 = new DateTime(2017, 4, 11, 12, 0, 0, DateTimeKind.Utc);
        public static DateTime Release_3_2 = new DateTime(2018, 8, 28, 10, 0, 0, DateTimeKind.Utc);
        public static DateTime Release_3_3 = new DateTime(2018, 12, 11, 9, 0, 0, DateTimeKind.Utc);
        public static DateTime Odyssey5 = new DateTime(2021, 7, 1,12,0,0, DateTimeKind.Utc);
        public static DateTime Odyssey14 = new DateTime(2022, 11, 29, 12, 0, 0, DateTimeKind.Utc);
    }
    public static class EliteFixesDates
    {
        public static DateTime ED_No_Training_Timestamp = new DateTime(2017, 10, 4, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime ED_No_Faction_Timestamp = new DateTime(2017, 9, 26, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime TotalEarningsCorrectDate = new DateTime(2018, 5, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    public static class EDDFixesDates
    {
        public static DateTime EDSMMinimumSystemsDate = new DateTime(2015, 5, 10, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime BookmarkUTCswitchover = new DateTime(2020, 1, 23, 0, 0, 0, DateTimeKind.Utc);
    }
}
