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
using EliteDangerousCore.DB;
using System;
using static BaseUtils.TypeHelpers;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.CodexEntry)]
    public class JournalCodexEntry : JournalEntry, IStarScan, IEquatable<JournalCodexEntry>
    {
        public JournalCodexEntry(JObject evt) : base(evt, JournalTypeEnum.CodexEntry)
        {
            EntryID = evt["EntryID"].Long();
            Name = evt["Name"].Str();
            Name_Localised = JournalFieldNaming.CheckLocalisation(evt["Name_Localised"].Str(), Name);
            SubCategory = evt["SubCategory"].Str();
            SubCategory_Localised = JournalFieldNaming.CheckLocalisation(evt["SubCategory_Localised"].Str(), SubCategory);
            Category = evt["Category"].Str();
            Category_Localised = JournalFieldNaming.CheckLocalisation(evt["Category_Localised"].Str(), Category);
            Region = evt["Region"].Str();
            Region_Localised = evt["Region_Localised"].Str();
            System = evt["System"].Str();
            SystemAddress = evt["SystemAddress"].LongNull();
            IsNewEntry = evt["IsNewEntry"].BoolNull();
            NewTraitsDiscovered = evt["NewTraitsDiscovered"].BoolNull();
            NearestDestination = evt["NearestDestination"].StrNull();
            if (!NearestDestination.HasChars())     // sometimes can be empty string
                NearestDestination = null;
            NearestDestination_Localised = JournalFieldNaming.CheckLocalisation(evt["NearestDestination_Localised"].StrNull(), NearestDestination);
            if ( evt["Traits"] != null )
                Traits = evt["Traits"].ToObjectQ<string[]>();
            VoucherAmount = evt["VoucherAmount"].LongNull();
            Latitude = evt["Latitude"].DoubleNull();        // odyssey
            Longitude = evt["Longitude"].DoubleNull();

            // EDD Additions
            EDDBodyName = evt["EDDBodyName"].StrNull();
            EDDBodyId = evt["EDDBodyID"].Int(-1);
        }

        [PropertyNameAttribute("Frontier entry ID")]
        public long EntryID { get; set; }
        [PropertyNameAttribute("Frontier Name")]
        public string Name { get; set; }
        [PropertyNameAttribute("Localised name")]
        public string Name_Localised { get; set; }
        [PropertyNameAttribute("FDName Category")]
        public string Category { get; set; }
        [PropertyNameAttribute("Localised category")]
        public string Category_Localised { get; set; }
        [PropertyNameAttribute("FDName Sub Category")]
        public string SubCategory { get; set; }
        [PropertyNameAttribute("Sub Category localised")]
        public string SubCategory_Localised { get; set; }
        [PropertyNameAttribute("FDName Region of galaxy")]
        public string Region { get; set; }
        [PropertyNameAttribute("Localised name of region")]
        public string Region_Localised { get; set; }
        [PropertyNameAttribute("System name")]
        public string System { get; set; }
        [PropertyNameAttribute("FD System address")]
        public long? SystemAddress { get; set; }
        [PropertyNameAttribute("Is it a new entry")]
        public bool? IsNewEntry { get; set; }
        [PropertyNameAttribute("Is traits discovered")]
        public bool? NewTraitsDiscovered { get; set; }
        [PropertyNameAttribute("Worth, cr")]
        public long? VoucherAmount { get; set; }
        [PropertyNameAttribute("Traits array")]
        public string [] Traits { get; set; }
        [PropertyNameAttribute("Nearest destination on planet if given")]
        public string NearestDestination { get; set; }
        [PropertyNameAttribute("Nearest destination localised")]
        public string NearestDestination_Localised { get; set; }
        [PropertyNameAttribute("Lattitude if on planet if given")]
        public double? Latitude { get; set; }
        [PropertyNameAttribute("Longitude if on planet if given")]
        public double? Longitude { get; set; }

        [PropertyNameAttribute("EDD assigned body name")]
        public string EDDBodyName { get; set; }        // EDD addition, filled in in ED. Null for not known
        [PropertyNameAttribute("EDD assigned body ID")]
        public int EDDBodyId { get; set; } = -1;       // EDD addition, filled in in ED.  -1 for not known

        public bool Equals(JournalCodexEntry other)
        {
            return Name.Equals(other.Name, StringComparison.CurrentCultureIgnoreCase) &&
                   SubCategory.Equals(other.SubCategory, StringComparison.CurrentCultureIgnoreCase) &&
                   Category.Equals(other.Category, StringComparison.CurrentCultureIgnoreCase) &&
                   Region.Equals(other.Region, StringComparison.CurrentCultureIgnoreCase) &&
                   SystemAddress == other.SystemAddress;
        }

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddCodexEntryToSystem(this);
        }

        public override void FillInformation(out string info, out string detailed)   
        {
            info = BaseUtils.FieldBuilder.Build("At ".T(EDCTx.JournalCodexEntry_At), System, ";", EDDBodyName, "ID", EDDBodyId, "in ".T(EDCTx.JournalCodexEntry_in), Region_Localised,
                                                "", Name_Localised,
                                                "", Category_Localised,
                                                "", SubCategory_Localised,
                                                ";New Entry".T(EDCTx.JournalCodexEntry_NewEntry), IsNewEntry,
                                                ";Traits".T(EDCTx.JournalCodexEntry_Traits), NewTraitsDiscovered,
                                                "Nearest: ".T(EDCTx.JournalEntry_Nearest), NearestDestination_Localised
                                                );
            if ( Latitude.HasValue )
                info += ", " + JournalFieldNaming.RLat(Latitude) + " " + JournalFieldNaming.RLong(Longitude);
            detailed = "";

            if (Traits != null)
                detailed = String.Join(",", Traits);
        }

        public string Info()
        {
            return BaseUtils.FieldBuilder.Build("", Region_Localised,
                                                "", Name_Localised,
                                                "", Category_Localised,
                                                "", SubCategory_Localised,
                                                ";New Entry".T(EDCTx.JournalCodexEntry_NewEntry), IsNewEntry,
                                                ";Traits".T(EDCTx.JournalCodexEntry_Traits), NewTraitsDiscovered,
                                                "Nearest: ".T(EDCTx.JournalEntry_Nearest), NearestDestination_Localised
                                                );
        }

        public void UpdateDB()
        {
            UserDatabase.Instance.DBWrite(cn =>
            {
                JObject jo = GetJson(cn);

                if (jo != null)
                {
                    jo["EDDBodyName"] = EDDBodyName;        // these are not in JSON from frontier, so add them in (or just overwrite them)
                    jo["EDDBodyID"] = EDDBodyId;
                    UpdateJsonEntry(jo, cn, null);
                }
            });

        }
    }
}