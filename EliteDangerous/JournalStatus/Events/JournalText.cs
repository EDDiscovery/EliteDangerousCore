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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EliteDangerousCore.JournalEvents
{
    [JournalEntryType(JournalTypeEnum.SendText)]
    public class JournalSendText : JournalEntry
    {
        public JournalSendText(JObject evt) : base(evt, JournalTypeEnum.SendText)
        {
            To = evt["To"].Str();
            To_Localised = JournalFieldNaming.CheckLocalisation(evt["To_Localised"].Str(),To);
            Message = evt["Message"].Str();
            Command = Message.StartsWith("/") && To.Equals("Local", StringComparison.InvariantCultureIgnoreCase);
        }

        public string To { get; set; }
        public string To_Localised { get; set; }
        public string Message { get; set; }
        public bool Command { get; set; }

        public override string GetInfo() 
        {
            return BaseUtils.FieldBuilder.Build("To: ".T(EDCTx.JournalSendText_To), To_Localised, "Msg: ".T(EDCTx.JournalSendText_Msg), Message);
        }
    }


    [JournalEntryType(JournalTypeEnum.ReceiveText)]
    public class JournalReceiveText : JournalEntry
    {
        public JournalReceiveText(JObject evt) : base(evt, JournalTypeEnum.ReceiveText)
        {
            From = evt["From"].Str();
            string loc = evt["From_Localised"].StrNull();       // is it present, if so, what is it
            FromLocalised = JournalFieldNaming.CheckLocalisation(loc??"", From);
            Message = evt["Message"].Str();
            MessageLocalised = JournalFieldNaming.CheckLocalisation(evt["Message_Localised"].Str(), Message);
            Channel = evt["Channel"].Str();

            string[] specials = new string[] { "$COMMS_entered:", "$CHAT_intro;", "$HumanoidEmote" };

            if ( specials.StartsWith(Message, System.StringComparison.InvariantCultureIgnoreCase)>=0)
            {
                Channel = "Info";
            }

            // some From's contain an ID without a localisation field, try and fix it
            if (loc == null && Channel.EqualsIIC("npc") && From.Contains("$"))
            {
                ItemData.Actor ac = ItemData.GetActorNPC(From);
                if (ac != null)
                    FromLocalised = ac.Name;
               // System.Diagnostics.Debug.WriteLine($"RT {EventTimeUTC} {Channel} `{From}` `{loc}`=> `{FromLocalised}` {evt.ToString()}");
            }
        }

        public string From { get; set; }
        public string FromLocalised { get; set; }
        public string Message { get; set; }
        public string MessageLocalised { get; set; }
        public string Channel { get; set; }         // wing/local/voicechat/friend/player/npc : 3.3 adds squadron/starsystem

        public List<JournalReceiveText> MergedEntries { get; set; }    // if verbose.. doing it this way does not break action packs as the variables are maintained
                                                                       // This is second, third merge etc.  First one is in above variables

        public override string GetInfo()
        {
            if (MergedEntries == null)
                return ToString();
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append((MergedEntries.Count() + 1).ToString());
                sb.Append(" Texts".T(EDCTx.JournalReceiveText_Text));
                sb.AppendSPC();
                sb.Append("from ".T(EDCTx.JournalReceiveText_FC));
                sb.Append(Channel);
                return sb.ToString();
            }
        }

        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (MergedEntries != null)
            {
                for (int i = MergedEntries.Count - 1; i >= 0; i--)
                    sb.AppendPrePad(MergedEntries[i].ToStringNC(), System.Environment.NewLine);
            }

            sb.AppendPrePad(ToStringNC(), System.Environment.NewLine);   // ours is the last one

            return sb.ToString();
        }    

        public override string ToString()
        {
            if ( FromLocalised.HasChars() )
                return BaseUtils.FieldBuilder.Build("From: ".T(EDCTx.JournalReceiveText_From), FromLocalised, "< on ".T(EDCTx.JournalReceiveText_on), Channel, "<: ", MessageLocalised);
            else
                return BaseUtils.FieldBuilder.Build("", Channel, "<: ", MessageLocalised);
        }

        public string ToStringNC()
        {
            return BaseUtils.FieldBuilder.Build("From: ".T(EDCTx.JournalReceiveText_From), FromLocalised, "<: ", MessageLocalised);
        }

        public void Add(JournalReceiveText next)
        {
            if (MergedEntries == null)
                MergedEntries = new List<JournalReceiveText>();
            MergedEntries.Add(next);
        }

    }

}
