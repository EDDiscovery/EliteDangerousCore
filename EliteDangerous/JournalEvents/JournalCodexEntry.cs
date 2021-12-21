/*
 * Copyright © 2016-2021 EDDiscovery development team
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
using BaseUtils.JSON;
using EliteDangerousCore.DB;
using System;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.CodexEntry)]
    public class JournalCodexEntry : JournalEntry, IStarScan
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
            NearestDestination_Localised = JournalFieldNaming.CheckLocalisation(evt["NearestDestination_Localised"].StrNull(), NearestDestination);
            if ( evt["Traits"] != null )
                Traits = evt["Traits"].ToObjectQ<string[]>();
            VoucherAmount = evt["VoucherAmount"].LongNull();
            Latitude = evt["Latitude"].Double();        // odyssey
            Longitude = evt["Longitude"].Double();

            // EDD Additions
            EDDBodyName = evt["EDDBodyName"].StrNull();
            EDDBodyId = evt["EDDBodyID"].Int(-1);
        }

        public long EntryID { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public string Category { get; set; }
        public string Category_Localised { get; set; }
        public string SubCategory { get; set; }
        public string SubCategory_Localised { get; set; }
        public string Region { get; set; }
        public string Region_Localised { get; set; }
        public string System { get; set; }
        public long? SystemAddress { get; set; }
        public bool? IsNewEntry { get; set; }
        public bool? NewTraitsDiscovered { get; set; }
        public long? VoucherAmount { get; set; }
        public string [] Traits { get; set; }
        public string NearestDestination { get; set; }
        public string NearestDestination_Localised { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string EDDBodyName { get; set; }        // EDD addition, filled in in ED. Null for not known
        public int EDDBodyId { get; set; } = -1;       // EDD addition, filled in in ED.  -1 for not known

        public void AddStarScan(StarScan s, ISystem system)
        {
            s.AddCodexEntryToSystem(this);
        }

        public override void FillInformation(ISystem sysunused, string whereamiunused, out string info, out string detailed)   
        {
            info = BaseUtils.FieldBuilder.Build("At ".T(EDTx.JournalCodexEntry_At), System, ";", EDDBodyName, "ID", EDDBodyId, "in ".T(EDTx.JournalCodexEntry_in), Region_Localised,
                                                "", Name_Localised,
                                                "", Category_Localised,
                                                "", SubCategory_Localised,
                                                ";New Entry".T(EDTx.JournalCodexEntry_NewEntry), IsNewEntry,
                                                ";Traits".T(EDTx.JournalCodexEntry_Traits), NewTraitsDiscovered,
                                                "Nearest: ".T(EDTx.JournalEntry_Nearest), NearestDestination_Localised
                                                );
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
                                                ";New Entry".T(EDTx.JournalCodexEntry_NewEntry), IsNewEntry,
                                                ";Traits".T(EDTx.JournalCodexEntry_Traits), NewTraitsDiscovered,
                                                "Nearest: ".T(EDTx.JournalEntry_Nearest), NearestDestination_Localised
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
                    UpdateJsonEntry(jo, cn);
                }
            });

        }
    }
}