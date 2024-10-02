/*
 * Copyright 2015-2022 EDDiscovery development team
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
using System.Linq;

namespace EliteDangerousCore
{
    public class EliteNameClassifier
    {
        public const string NoSectorName = "NoSectorName";        // Sol etc

        public enum NameType        // describes the NID
        {
            NotSet,         // not set
            Named,          // Named type (SOL or HIP-1234) 
            Numeric,        // numeric name (29282-2902)
            Identifier,     // Pru Eurk CQ-L                                SectorName = sector, StarName = null, L1L2L3 set
            Masscode,       // Pru Eurk CQ-L d                              SectorName = sector, StarName = null, L1L2L3 set, MassCode set
            NValue,         // Pru Eurk CQ-L d2-3 or Pru Eurk CQ-L d2       SectorName = sector, StarName = null, L1L2L3 set, MassCode set, NValue set
            N1ValueOnly     // Pru Eurk CQ-L d2-                            SectorName = sector, StarName = null, L1L2L3 set, MassCode set, NValue set
        };

        public NameType EntryType { get; private set; } = NameType.NotSet;
        public string SectorName { get; set; } = null;    // for string inputs, set always. Either the sector name (Pru Eurk or survey HIP etc) or "NotInSector" for Named (Sol). For id input, null
        public string StarName { get; set; } = null;      // for string inputs, set for surveys or non standard names, else null. For id input, null
        public ulong NameIdNumeric { get; set; } = 0;      // for string inputs: if its a numeric, value, else 0. For id input, NIndex into name table (for Sol) or numeric name (for 12345=56) else zero

        private uint L1, L2, L3, MassCode, NValue;   // set for standard names
        private uint NumericDashPos = 0;     // Numeric Dash position
        private uint NumericDigits = 0;      // Numeric digits

        //   6    5    5    4  4444 4444    3    3    2    2    2    1    1      
        //   0    6    2    8  7654 3210    6    2    8    4    0    6    2    8    4    0
        //0000 0000 0000 0000  1000 0111 1122 2223 3333 MMMM N111 1111 N222 2222 2222 2222                  - older style - Bit 47 is marker
        //0010 0000 0000 0000  0000 0111 1122 2223 3333 MMMM N111 1111 N222 2222 2222 2222                  - newer style - bit 61 is marker
        //   F    F    F    F     F    8    0    0    0    0    0    0    0    0    0    0
        private static int L1L2L3Marker = 47;       // Standard (L1/Mass/N apply).   47 means its in 6 bytes, fitting within a 6 byte SQL field
        private const int L1Marker = 38;            // Standard: 5 bits 38-42 (1 = A, 26=Z)
        private const int L2Marker = 33;            // Standard: 5 bits 33-37 (1 = A, 26=Z)
        private const int L3Marker = 28;            // Standard: 5 bits 28-32 (aligned for display purposes) (1 = A, 26=Z)
        private const int MassMarker = 24;          // Standard: 3 bits 24-27 (0=A,7=H)
        private const int NMarker = 0;              // Standard: N2 + N1<<16  

        //   6    5    5    4  4444 4444    3    3    2    2    2    1    1      
        //   0    6    2    8  7654 3210    6    2    8    4    0    6    2    8    4    0
        //0000 0000 0000 0000  01CC CCDD DDNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN                  - older style - bit 46 is marker           
        //0001 0000 0000 0000  00CC CCDD DDNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN                  - newer style - bit 60 is marker           
        //   F    F    F    F     C    0    0    0    0    0    0    0    0    0    0    0
        private static int NumericMarker = 46;       // Numeric (HIP 1232-23). bits 0-35 hold value.  
        private const int NumericCountMarker = 42;  // Numeric: 4 bits 42-45 Number of digits in number
        private const int NumericDashMarker = 38;   // Numeric: 4 bits 38-41 position of dash in number (0 = none, 1 = 0 char in, 2 = 1 char in etc) 

        //   6    5    5    4  4444 4444    3    3    2    2    2    1    1      
        //   0    6    2    8  7654 3210    6    2    8    4    0    6    2    8    4    0
        //0000 0000 0000 0000  00NN NNNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN                  - older style - Bit 46 and 47 is zero
        //0000 0000 0000 0000  00NN NNNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN NNNN                  - newer style - bit 60 and 61 is zero
        //   0    0    0    0     3    F    F    F    F    F    F    F    F    F    F    F
        private const long NameIDNumbericMask = 0x3fffffffff;     // 38 bits

        public bool IsStandard { get { return EntryType >= NameType.NValue; } }     // meaning L1L2L3 MassCode NValue is set..
        public bool IsStandardParts { get { return EntryType >= NameType.Identifier; } }    // is L1L2L3 is set at least
        public bool IsNamed { get { return EntryType == NameType.Named; } }     // name, such as Sol
        public bool IsNumeric { get { return EntryType == NameType.Numeric; } } // numberic, such as HIP1234-33

        public static bool IsIDStandard(ulong id)       // if encoded ID a standard masscode type
        {
            return (id & (1UL << L1L2L3Marker)) != 0;
        }
        public static bool IsIDNumeric(ulong id)        // if encoded ID a numeric marker
        {
            return (id & (1UL << NumericMarker)) != 0;
        }

        // Turn the data in the class into an ID code (which goes into the DB in the nameid field)
        public ulong ID 
        {
            get
            {
                if (IsStandardParts)
                {
                    System.Diagnostics.Debug.Assert(L1 < 31 && L2 < 32 && L3 < 32 && NValue < 0xffffff && MassCode < 8);
                    return ((ulong)NValue << NMarker) | ((ulong)(MassCode) << MassMarker) | ((ulong)(L3) << L3Marker) | ((ulong)(L2) << L2Marker) | ((ulong)(L1) << L1Marker) | (1UL << L1L2L3Marker);
                }
                else if (IsNumeric)
                {
                    return (ulong)(NameIdNumeric) | (1UL << NumericMarker) | ((ulong)(NumericDashPos) << NumericDashMarker) | ((ulong)(NumericDigits) << NumericCountMarker);
                }
                else
                {
                    return (ulong)(NameIdNumeric);
                }
            }
        }

        public ulong IDHigh // get the ID code, giving parts not set the maximum value.  Useful for wildcard searches when code has been set by a string
        {
            get
            {
                if (IsStandardParts)
                {
                    ulong lcodes = ((ulong)(L3) << L3Marker) | ((ulong)(L2) << L2Marker) | ((ulong)(L1) << L1Marker) | (1UL << L1L2L3Marker);

                    if (EntryType == NameType.Identifier)
                        return ((1UL << L3Marker) - 1) | lcodes;

                    lcodes |= ((ulong)(MassCode) << MassMarker);

                    if (EntryType == NameType.Masscode)
                        return ((1UL << MassMarker) - 1) | lcodes;

                    if (EntryType == NameType.N1ValueOnly)
                        return lcodes | ((ulong)NValue << NMarker) | 0xffff; // N1 explicit (d23-) then we can assume a wildcard in the bottom N2
                    else
                        return lcodes | ((ulong)NValue << NMarker); // no wild card here
                }
                else 
                    return ID;
            }
        }


        public override string ToString()
        {
            if (IsStandard)
            {
                return (SectorName != null ? (SectorName + " ") : "") + (char)(L1 + 'A' - 1) + (char)(L2 + 'A' - 1) + "-" + (char)(L3 + 'A' - 1) + " " + (char)(MassCode + 'a') + (NValue > 0xffff ? ((NValue / 0x10000).ToStringInvariant() + "-") : "") + (NValue & 0xffff).ToStringInvariant();
            }
            else if (IsNumeric)
            {
                string num = NameIdNumeric.ToStringInvariant("0000000000000000".Substring(0, (int)NumericDigits));
                if (NumericDashPos > 0)
                    num = num.Substring(0, (int)(NumericDashPos - 1)) + "-" + num.Substring((int)(NumericDashPos - 1));

                return (SectorName != null && SectorName != NoSectorName ? (SectorName + " ") : "") + num;
            }
            else
                return (SectorName != null && SectorName != NoSectorName ? (SectorName + " ") : "") + StarName;
        }

        public EliteNameClassifier()
        {
        }

        public EliteNameClassifier(string n)
        {
            Classify(n);
        }

        // Create from ID
        public EliteNameClassifier(ulong id)        
        {
            Classify(id);
        }

        public static void ChangeToNewBitPositions()       // fixes issue with systemid being 56 bits long, and thus impacting bit 46/47 markers
        {
            L1L2L3Marker = 61;
            NumericMarker = 60;
        }

        // take the ID from the DB and turn it back into parts
        public void Classify(ulong id)              
        {
            if (IsIDStandard(id))       // ID has standard L1L2L3 masscode NValues
            {
                NValue = (uint)(id >> NMarker) & 0xffffff;
                MassCode = (char)(((id >> MassMarker) & 7));
                L3 = (char)(((id >> L3Marker) & 31));
                L2 = (char)(((id >> L2Marker) & 31));
                L1 = (char)(((id >> L1Marker) & 31));
                EntryType = NameType.NValue;
                System.Diagnostics.Debug.Assert(L1 < 31 && L2 < 32 && L3 < 32 && NValue < 0xffffff && MassCode < 8);
            }
            else if (IsIDNumeric(id))   // 192929-290
            {
                NameIdNumeric = (ulong)(id & NameIDNumbericMask);
                NumericDashPos = (uint)((id >> NumericDashMarker) & 15);
                NumericDigits = (uint)((id >> NumericCountMarker) & 15);
                EntryType = NameType.Numeric;
            }
            else
            {
                NameIdNumeric = (ulong)(id & NameIDNumbericMask);        // set the index into the name table
                EntryType = NameType.Named;
            }

            SectorName = StarName = null;
        }

        // classify a string
        // starname is case sensitive and case preserving
        public void Classify(string starname)
        {
            EntryType = NameType.NotSet;

            string[] nameparts = starname.Split(' ');

            L1 = L2 = L3 = MassCode = NValue = 0;      // unused parts are zero

            for (int i = 0; i < nameparts.Length; i++)
            {
                if (i > 0 && nameparts[i].Length == 4 && nameparts[i][2] == '-' && char.IsLetter(nameparts[i][0]) && char.IsLetter(nameparts[i][1]) && char.IsLetter(nameparts[i][3]))
                {
                    L1 = (uint)(char.ToUpper(nameparts[i][0]) - 'A' + 1);
                    L2 = (uint)(char.ToUpper(nameparts[i][1]) - 'A' + 1);
                    L3 = (uint)(char.ToUpper(nameparts[i][3]) - 'A' + 1);

                    EntryType = NameType.Identifier;

                    if (nameparts.Length > i + 1)
                    {
                        string p = nameparts[i + 1];

                        if (p.Length > 0)           // should have a or a56-229 or a56
                        {
                            char mc = char.ToLower(p[0]);
                            if (mc >= 'a' && mc <= 'h')
                            {
                                MassCode = (uint)(mc - 'a');
                                EntryType = NameType.Masscode;

                                if (p.Length > 1)       // if we have text after, needs to be x or x-y.  No text, just masscode
                                {
                                    int slash = p.IndexOf("-");
                                    int? first = (slash >= 0 ? p.Substring(1, slash - 1) : p.Substring(1)).InvariantParseIntNull();

                                    if (first >= 0)     // must be a number which converted positive, else its a unique name
                                    {
                                        if (slash > 0)     // if we have a slash
                                        {
                                            string spart = p.Substring(slash + 1);  // the slash part

                                            if (spart.Length > 0)     // if present
                                            {
                                                int? second = spart.InvariantParseIntNull();

                                                if (first >= 0 && first < 256 && second >= 0 && second < 65536)    // first and second part must be a number in range
                                                {
                                                    NValue = (uint)first * 0x10000 + (uint)second;
                                                    EntryType = NameType.NValue;
                                                }
                                                else
                                                    EntryType = NameType.NotSet; // abandon, treat as a unique name
                                            }
                                            else
                                            {
                                                // just first, not second, so d29-
                                                if (first >= 0 && first < 256)
                                                {
                                                    NValue = (uint)first * 0x10000;
                                                    EntryType = NameType.N1ValueOnly;
                                                }
                                                else
                                                    EntryType = NameType.NotSet; // abandon, treat as a unique name
                                            }
                                        }
                                        else
                                        {       /// no slash, just a number
                                            if (first >= 0 && first < 65536)
                                            {
                                                NValue = (uint)first;
                                                EntryType = NameType.NValue;
                                            }
                                            else
                                                EntryType = NameType.NotSet; // abandon, treat as a unique name
                                        }
                                    }
                                    else
                                        EntryType = NameType.NotSet; // abandon, treat as a unique name
                                }
                            }
                        }
                    }

                    SectorName = nameparts[0];
                    for (int j = 1; j < i; j++)
                        SectorName = SectorName + " " + nameparts[j];

                    StarName = null;
                    break;
                } // end if
            }

            if (EntryType == NameType.NotSet)
            {
                string[] surveys = new string[] { "2MASS", "HD", "LTT", "TYC", "NGC", "HR", "LFT", "LHS", "LP", "Wolf", "IHA2007", "USNO-A2.0", "2547" , "DBP2006" , "NOMAD1", "OJV2009" , "PSR",
                                                "SSTGLMC", "StKM", "UGCS"};

                int dashpos = 0;
                ulong? namenum = null;
                int countof = 0;

                if (nameparts.Length >= 2)     // see if last is a number or number-number
                {
                    dashpos = nameparts.Last().IndexOf('-');
                    string num = (dashpos >= 0 && nameparts.Last().Count(x => x == '-') == 1) ? nameparts.Last().Replace("-", "") : nameparts.Last();
                    namenum = num.InvariantParseULongNull();
                    countof = num.Length;

                    if (namenum.HasValue && namenum.Value > NameIDNumbericMask)
                        System.Diagnostics.Debug.WriteLine("Starname " + starname + " too big");
                }

                if (namenum.HasValue && namenum.Value <= NameIDNumbericMask)  // if numeric and fits within mask size
                {
                    NumericDashPos = (uint)(dashpos + 1);       // record dash pos AND count of digits (lots have leading zeros)
                    NumericDigits = (uint)countof;

                    SectorName = string.Join(" ", nameparts.RangeSubset(0, nameparts.Length - 1));
                    NameIdNumeric = namenum.Value;
                    EntryType = NameType.Numeric;
                }
                else
                {
                    if (surveys.Contains(nameparts[0], StringComparer.InvariantCultureIgnoreCase))
                    {
                        SectorName = nameparts[0];
                        StarName = starname.Mid(nameparts[0].Length + 1).Trim();
                    }
                    else
                    {
                        SectorName = NoSectorName;
                        StarName = starname.Trim();
                    }

                    EntryType = NameType.Named;
                }

                System.Diagnostics.Debug.Assert(ToString().Trim() == starname.Trim(), $"ENC Non compare '{ToString()}' to '{starname}'");        // double check conversion
            }
        }
    }
}
