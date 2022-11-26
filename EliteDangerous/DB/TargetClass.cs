/*
 * Copyright 2016-2022 EDDiscovery development team
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

namespace EliteDangerousCore.DB
{
    // Targets are associated with a bookmark, a system, or a gmo object from nov 22
    public class TargetClass
    {
        public enum TargetType { Bookmark, NoteUnused, GMO, None , Star };

        public static void SetTargetOnBookmark(string name, long id, double x, double y, double z)
        {
            UserDatabase.Instance.PutSettingString("TargetPositionName", name);
            UserDatabase.Instance.PutSettingInt("TargetPositionType", (int)TargetType.Bookmark);
            UserDatabase.Instance.PutSettingInt("TargetPositionID", (int)id);
            UserDatabase.Instance.PutSettingDouble("TargetPositionX", x);
            UserDatabase.Instance.PutSettingDouble("TargetPositionY", y);
            UserDatabase.Instance.PutSettingDouble("TargetPositionZ", z);
        }

        public static void SetTargetOnSystem(string name, double x, double y, double z)
        {
            UserDatabase.Instance.PutSettingString("TargetPositionName", name);
            UserDatabase.Instance.PutSettingInt("TargetPositionType", (int)TargetType.Star);
            UserDatabase.Instance.PutSettingInt("TargetPositionID", -1);
            UserDatabase.Instance.PutSettingDouble("TargetPositionX", x);
            UserDatabase.Instance.PutSettingDouble("TargetPositionY", y);
            UserDatabase.Instance.PutSettingDouble("TargetPositionZ", z);
        }

        public static bool SetTargetOnSystemConditional(string name, double x, double y, double z)
        {
            string tname = EliteDangerousCore.DB.UserDatabase.Instance.GetSettingString("TargetPositionName", "");
            TargetType tt = (TargetType)EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("TargetPositionType", (int)TargetType.None);
            if (tt != TargetType.Star || tname != name)
            {
                SetTargetOnSystem(name, x, y, z);
                return true;
            }
            else
                return false;
        }

        public static void SetTargetOnGMO(string name, long id, double x, double y, double z) 
        {
            UserDatabase.Instance.PutSettingString("TargetPositionName", name);
            UserDatabase.Instance.PutSettingInt("TargetPositionType", (int)TargetType.GMO);
            UserDatabase.Instance.PutSettingInt("TargetPositionID", (int)id);
            UserDatabase.Instance.PutSettingDouble("TargetPositionX", x);
            UserDatabase.Instance.PutSettingDouble("TargetPositionY", y);
            UserDatabase.Instance.PutSettingDouble("TargetPositionZ", z);
        }

        public static void ClearTarget()
        {
            EliteDangerousCore.DB.UserDatabase.Instance.PutSettingInt("TargetPositionType", (int)TargetType.None);
        }

        public static long GetTargetBookmarkID()      // -1 if not a bookmark or not set.
        {
            TargetType tt = (TargetType)EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("TargetPositionType", (int)TargetType.None);
            return (tt == TargetType.Bookmark) ? EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("TargetPositionID", 0) : -1;
        }

        // true if target set with its name, x/y/z.  if not set, name is null, xyz is nan
        public static bool GetTargetPosition(out string name, out double x, out double y, out double z)
        {
            name = null;
            x = y = z = double.NaN;

            if (IsTargetSet())      // use the interface
            {
                name = UserDatabase.Instance.GetSettingString("TargetPositionName", "");
                x = UserDatabase.Instance.GetSettingDouble("TargetPositionX", double.NaN);
                y = UserDatabase.Instance.GetSettingDouble("TargetPositionY", double.NaN);
                z = UserDatabase.Instance.GetSettingDouble("TargetPositionZ", double.NaN);
                return true;
            }
            else
                return false;
        }

        public static bool IsTargetSet()
        {
            TargetType tt = (TargetType)EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("TargetPositionType", (int)TargetType.None);
            return tt == TargetType.Star || tt == TargetType.GMO || tt == TargetType.Bookmark;          // some are now depreciated, so explicity check types allowed
        }

        public static TargetType GetTargetType()
        {
            TargetType tt = (TargetType)EliteDangerousCore.DB.UserDatabase.Instance.GetSettingInt("TargetPositionType", (int)TargetType.None);
            return tt;
        }
    }

}
