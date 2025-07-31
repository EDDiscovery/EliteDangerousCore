/*
 * Copyright © 2025-2025 EDDiscovery development team
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.ColonisationConstructionDepot)]
    public class JournalColonisationConstructionDepot : JournalEntry, IEquatable<JournalColonisationConstructionDepot>
    {
        public JournalColonisationConstructionDepot(JObject evt) : base(evt, JournalTypeEnum.ColonisationConstructionDepot)
        {
            evt.ToObjectProtected(this.GetType(), true,
                membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                initialobject: this,
                customformat: (ty, ob) => { return JournalFieldNaming.FixCommodityName((string)ob); }
                );        // read fields named in this structure matching JSON names
        }
        public long MarketID { get; set; }
        public float ConstructionProgress { get; set; }
        public bool ConstructionComplete { get; set; }
        public bool ConstructionFailed { get; set; }

        [System.Diagnostics.DebuggerDisplay("{Name_Localised} {RequiredAmount} {ProvidedAmount} {Payment}")]
        public class ResourcesList : IEquatable<ResourcesList>
        {
            [JsonCustomFormat]
            public string Name { get; set; }        // fdname
            public string Name_Localised { get; set; }
            public int RequiredAmount { get; set; }
            public int ProvidedAmount { get; set; }
            public long Payment { get; set; }

            public bool Equals(ResourcesList other)
            {
                return Name == other.Name && RequiredAmount == other.RequiredAmount && ProvidedAmount == other.ProvidedAmount &&
                        Payment == other.Payment;
            }
        }

        public ResourcesList[] ResourcesRequired { get; set; }

        public bool Equals(JournalColonisationConstructionDepot other)
        {
            return other.MarketID == MarketID &&
                 other.ConstructionProgress == ConstructionProgress &&
                 other.ConstructionComplete == ConstructionComplete &&
                other.ConstructionFailed == ConstructionFailed &&
                other.ResourcesRequired.SequenceEqual(ResourcesRequired);
        }

        public override string GetInfo(JournalEntry.FillInformationData fid)
        {
            return BaseUtils.FieldBuilder.Build("" , fid.BodyName, "Progress: ;%;N1".Tx(), ConstructionProgress * 100.0f,
                                                ";Complete", ConstructionComplete,
                                                ";Failed", ConstructionFailed);
        }

        public override string GetDetailed()
        {
            StringBuilder sb = new StringBuilder(1000);
            foreach (ResourcesList x in ResourcesRequired.EmptyIfNull())
            {
                sb.Append(MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(x.Name));
                sb.Append(": ");
                sb.Append(x.ProvidedAmount);
                sb.Append(" / ");
                sb.Append(x.RequiredAmount);
                sb.AppendCR();
            }

            return sb.ToString();
        }
    }


    [JournalEntryType(JournalTypeEnum.ColonisationContribution)]
    public class JournalColonisationContribution : JournalEntry
    {
        public JournalColonisationContribution(JObject evt) : base(evt, JournalTypeEnum.ColonisationContribution)
        {
            evt.ToObjectProtected(this.GetType(), true,
               membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
               initialobject: this,
                customformat: (ty, ob) => { return JournalFieldNaming.FixCommodityName((string)ob); }
               );        // read fields named in this structure matching JSON names
        }
        public long MarketID { get; set; }

        public class Contribution
        {
            [JsonCustomFormat]
            public string Name { get; set; }        // fdname
            public string Name_Localised { get; set; }
            public int Amount { get; set; }
        }

        public Contribution[] Contributions { get; set; }

        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder(1000);
            foreach (Contribution x in Contributions.EmptyIfNull())
            {
                sb.Append(MaterialCommodityMicroResourceType.GetTranslatedNameByFDName(x.Name));
                sb.Append(": ");
                sb.Append(x.Amount);
                sb.Append(" \r\n"); // space for non word wrapped
            }

            return sb.ToString();
        }
    }



    [JournalEntryType(JournalTypeEnum.ColonisationSystemClaim)]
    public class JournalColonisationSystemClaim : JournalEntry
    {
        public JournalColonisationSystemClaim(JObject evt) : base(evt, JournalTypeEnum.ColonisationSystemClaim)
        {
            evt.ToObjectProtected(this.GetType(), true,
                membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                initialobject: this
                );        // read fields named in this structure matching JSON names
        }

        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }

        public override string GetInfo()
        {
            return StarSystem;
        }
    }


    [JournalEntryType(JournalTypeEnum.ColonisationBeaconDeployed)]
    public class JournalColonisationBeaconDeployed : JournalEntry
    {
        public JournalColonisationBeaconDeployed(JObject evt) : base(evt, JournalTypeEnum.ColonisationBeaconDeployed)
        {
            // no data
        }
    }

    [JournalEntryType(JournalTypeEnum.ColonisationSystemClaimRelease)]
    public class JournalColonisationSystemClaimRelease : JournalEntry
    {
        public JournalColonisationSystemClaimRelease(JObject evt) : base(evt, JournalTypeEnum.ColonisationSystemClaimRelease)
        {
            evt.ToObjectProtected(this.GetType(), true,
                membersearchflags: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly,
                initialobject: this
                );        // read fields named in this structure matching JSON names
        }

        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }
    }


}
