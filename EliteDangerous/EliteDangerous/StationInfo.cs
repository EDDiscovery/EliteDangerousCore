/*
 * Copyright © 2023-2023 EDDiscovery development team
 *
 * Licensed under the Apache License", Version 2.0 (the "License")"] = "you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing", software distributed under
 * the License is distributed on an "AS IS" BASIS", WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND", either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using ExtendedControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EliteDangerousCore
{
    // StationInfo adds onto to JournalDocked more fields

    [System.Diagnostics.DebuggerDisplay("Station {System.Name} {BodyName} {StationName}")]
    public class StationInfo : JournalEvents.JournalDocked
    {
        public StationInfo(System.DateTime utc) : base(utc)
        {
        }
        public double DistanceRefSystem { get; set; }
        public ISystem System { get; set; }
        public string BodyName { get; set; }
        public string BodyType { get; set; }
        public string BodySubType { get; set; }
        public double DistanceToArrival { get; set; }
        public bool IsPlanetary { get; set; }
        public bool IsFleetCarrier { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public PowerPlayDefinitions.State PowerplayState { get; set; }      // Spansh gives this in search stations

        public bool HasMarket { get; set; }                 // may be true because its noted in services, but with market = null because of no data
        public List<CCommodities> Market { get; set; }      // may be null
        public DateTime MarketUpdateUTC { get; set; }
        public double MarketAgeInDays { get { return DateTime.UtcNow.Subtract(MarketUpdateUTC).TotalDays; } }
        public string MarketStateString { get { if (HasMarket && Market != null) return $"\u2713 {MarketAgeInDays:N1}"; else if (HasMarket) return "\u2713 ND"; else return ""; } }
        public CCommodities GetItem(string fdname) { return Market?.Find(x => x.fdname.Equals(fdname, StringComparison.InvariantCultureIgnoreCase)); }
        public string GetItemPriceString(string fdname, bool selltostation)
        {
            var entry = Market?.Find(x => x.fdname.Equals(fdname, StringComparison.InvariantCultureIgnoreCase));
            if (entry != null)
                return selltostation ? (entry.sellPrice.ToString("N0")) : (entry.buyPrice.ToString("N0"));
            else
                return "";
        }
        public string GetItemStockDemandString(string fdname, bool selltostation)
        {
            var entry = Market?.Find(x => x.fdname.Equals(fdname, StringComparison.InvariantCultureIgnoreCase));
            if (entry != null)
                return selltostation ? (entry.demand.ToString("N0")) : (entry.stock.ToString("N0"));
            else
                return "";
        }

        public string GetItemString(string fdname)
        {
            var entry = Market?.Find(x => x.fdname.Equals(fdname, StringComparison.InvariantCultureIgnoreCase));
            if (entry != null)
                return $"Category: {entry.loccategory}{Environment.NewLine}Sell to Station Price: {entry.sellPrice:N0}{Environment.NewLine}Demand: {entry.demand:N0}" +
                    $"{Environment.NewLine}Buy from Station Price: {entry.buyPrice:N0}{Environment.NewLine}Stock: {entry.stock:N0}";
            else
                return null;
        }

        // sync with journalcarrier..
        public bool HasItem(string fdname) { return Market != null && Market.FindIndex(x => x.fdname.Equals(fdname, StringComparison.InvariantCultureIgnoreCase)) >= 0; }
        public bool HasItemInStock(string fdname) { return Market != null && Market.FindIndex(x => x.fdname.Equals(fdname, StringComparison.InvariantCultureIgnoreCase) && x.HasStock) >= 0; }
        public bool HasItemWithDemandAndPrice(string fdname) { return Market != null && Market.FindIndex(x => x.fdname.Equals(fdname, StringComparison.InvariantCultureIgnoreCase) && x.HasDemandAndPrice) >= 0; }

        // go thru the market array, and see if any of the fdnames given matches that market entry
        public bool HasAnyItem(string[] fdnames) { return Market != null && Market.FindIndex(x => fdnames.Equals(x.fdname, StringComparison.InvariantCultureIgnoreCase) >= 0) >= 0; }
        public bool HasAnyItemInStock(string[] fdnames) { return Market != null && Market.FindIndex(x => fdnames.Equals(x.fdname, StringComparison.InvariantCultureIgnoreCase) >= 0 && x.HasStock) >= 0; }
        public bool HasAnyItemWithDemandAndPrice(string[] fdnames) { return Market != null && Market.FindIndex(x => fdnames.Equals(x.fdname, StringComparison.InvariantCultureIgnoreCase) >= 0 && x.HasDemandAndPrice) >= 0; }

        public bool HasOutfitting { get; set; }// see market
        public List<Outfitting.OutfittingItem> Outfitting { get; set; }     // may be null
        public DateTime OutfittingUpdateUTC { get; set; }
        public bool HasAnyModuleTypes(string[] fdnames) { return Outfitting != null && Outfitting.FindIndex(x => fdnames.Equals(x.FDName, StringComparison.InvariantCultureIgnoreCase) >= 0) >= 0; }
        public double OutfittingAgeInDays { get { return DateTime.UtcNow.Subtract(OutfittingUpdateUTC).TotalDays; } }
        public string OutfittingStateString { get { if (HasOutfitting && Outfitting != null) return $"\u2713 {OutfittingAgeInDays:N1}"; else if (HasOutfitting) return "\u2713 ND"; else return ""; } }

        public bool HasShipyard { get; set; }   // see market
        public List<ShipYard.ShipyardItem> Shipyard { get; set; }     // may be null
        public DateTime ShipyardUpdateUTC { get; set; }
        public bool HasAnyShipTypes(string[] fdnames) { return Shipyard != null && Shipyard.FindIndex(x => fdnames.Equals(x.ShipType, StringComparison.InvariantCultureIgnoreCase) >= 0) >= 0; }
        public double ShipyardAgeInDays { get { return DateTime.UtcNow.Subtract(ShipyardUpdateUTC).TotalDays; } }
        public string ShipyardStateString { get { if (HasShipyard && Shipyard != null) return $"\u2713 {ShipyardAgeInDays:N1}"; else if (HasShipyard) return "\u2713 ND"; else return ""; } }

        public override string GetInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Build("Station:", StationName, "System:", System.Name, "Body:", BodyName, "Lat:;;N4", Latitude, "Long:;;N4", Longitude, "Distance to Arrival:;ls;N1", DistanceToArrival);
            sb.AppendPrePad(base.GetInfo(), global::System.Environment.NewLine);
            return sb.ToString();
        }

        public override string GetDetailed()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(base.GetDetailed());

            if (HasMarket)
            {
                sb.AppendCR();
                sb.Append("Market: ");
                sb.AppendCR();

                foreach (CCommodities m in Market.EmptyIfNull())
                {
                    sb.Append(" " + m.ToStringShort());
                }
            }

            if (HasOutfitting)
            {
                sb.AppendCR();
                sb.Append("Outfitting: ");
                sb.AppendCR();
                foreach (var o in Outfitting.EmptyIfNull())
                {
                    sb.Append(" " + o.ToStringShort());
                    sb.AppendCR();
                }
            }
            if (HasShipyard)
            {
                sb.AppendCR();
                sb.Append("Shipyard: ");
                sb.AppendCR();
                foreach (var s in Shipyard.EmptyIfNull())
                {
                    sb.Append(" " + s.ToStringShort());
                    sb.AppendCR();
                }
            }

            return sb.ToString();
        }

        public void ViewMarket(Form fm, DB.IUserDatabaseSettingsSaver saver)
        {
            StationInfo si = this;

            var dgvpanel = new ExtPanelDataGridViewScrollWithDGV<BaseUtils.DataGridViewColumnControl>();
            dgvpanel.DataGrid.CreateTextColumns("Category", 100, 5,
                                                "Name", 150, 5,
                                                "Station Sells", 50, 5,
                                                "Stock", 50, 5,
                                                "Station Buys", 50, 5,
                                                "Demand", 50, 5
                                                );

            dgvpanel.DataGrid.SortCompare += (s, ev) => { if (ev.Column.Index >= 2) ev.SortDataGridViewColumnNumeric(); };
            dgvpanel.DataGrid.RowHeadersVisible = false;

            saver.DGVLoadColumnLayout(dgvpanel.DataGrid, "ShowMarket");

            foreach (var commd in si.Market.EmptyIfNull())
            {
                object[] rowobj = { commd.loccategory,
                    commd.locName,
                    commd.buyPrice.ToString("N0"),
                    commd.stock.ToString("N0"),
                    commd.sellPrice.ToString("N0"),
                    commd.demand.ToString("N0")
                };
                var row = dgvpanel.DataGrid.RowTemplate.Clone() as DataGridViewRow;
                row.CreateCells(dgvpanel.DataGrid, rowobj);
                dgvpanel.DataGrid.Rows.Add(row);
            }

            ConfigurableForm f = new ConfigurableForm();
            f.Add(new ConfigurableEntryList.Entry(dgvpanel, "Grid", "", new System.Drawing.Point(3, 30), new System.Drawing.Size(800, 400), null)
            { Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom });
            f.AddOK(new Point(800 - 100, 460), "OK", anchor: AnchorStyles.Right | AnchorStyles.Bottom);
            f.InstallStandardTriggers();
            f.AllowResize = true;

            string title = $"Commodities for {si.StationName}" + (si.MarketUpdateUTC.Year > 2000 ? " " + EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(si.MarketUpdateUTC).ToString() : "No Data");
            f.ShowDialogCentred(fm,fm.Icon, title, closeicon: true);

            saver.DGVSaveColumnLayout(dgvpanel.DataGrid, "ShowMarket");

        }


        public void ViewOutfitting(Form fm, DB.IUserDatabaseSettingsSaver saver)
        {
            StationInfo si = this;

            var dgvpanel = new ExtPanelDataGridViewScrollWithDGV<BaseUtils.DataGridViewColumnControl>();
            dgvpanel.DataGrid.CreateTextColumns("Category", 100, 5,
                                                "Name", 150, 5);

            dgvpanel.DataGrid.RowHeadersVisible = false;

            saver.DGVLoadColumnLayout(dgvpanel.DataGrid, "ShowOutfitting");

            foreach (var oi in si.Outfitting.EmptyIfNull())
            {
                object[] rowobj = { oi.TranslatedModTypeString, oi.TranslatedModuleName };
                var row = dgvpanel.DataGrid.RowTemplate.Clone() as DataGridViewRow;
                row.CreateCells(dgvpanel.DataGrid, rowobj);
                dgvpanel.DataGrid.Rows.Add(row);
            }

            ConfigurableForm f = new ConfigurableForm();
            f.Add(new ConfigurableEntryList.Entry(dgvpanel, "Grid", "", new System.Drawing.Point(3, 30), new System.Drawing.Size(800, 400), null)
            { Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom });
            f.AddOK(new Point(800 - 100, 460), "OK", anchor: AnchorStyles.Right | AnchorStyles.Bottom);
            f.InstallStandardTriggers();
            f.AllowResize = true;

            string title = "Outfitting for " + si.StationName + " " + (si.OutfittingUpdateUTC.Year > 2000 ? " " + EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(si.OutfittingUpdateUTC).ToString() : "No Data");
            f.ShowDialogCentred(fm,fm.Icon, title, closeicon: true);

            saver.DGVSaveColumnLayout(dgvpanel.DataGrid, "ShowOutfitting");

        }

        public void ViewShipyard(Form fm, DB.IUserDatabaseSettingsSaver saver)
        {
            StationInfo si = this;

            var dgvpanel = new ExtPanelDataGridViewScrollWithDGV<BaseUtils.DataGridViewColumnControl>();
            dgvpanel.DataGrid.CreateTextColumns("Name", 100, 5);

            dgvpanel.DataGrid.RowHeadersVisible = false;

            saver.DGVLoadColumnLayout(dgvpanel.DataGrid, "ShowShipyard");

            foreach (var oi in si.Shipyard.EmptyIfNull())
            {
                object[] rowobj = { oi.ShipType_Localised,
                                    };
                var row = dgvpanel.DataGrid.RowTemplate.Clone() as DataGridViewRow;
                row.CreateCells(dgvpanel.DataGrid, rowobj);
                dgvpanel.DataGrid.Rows.Add(row);
            }

            ConfigurableForm f = new ConfigurableForm();
            f.Add(new ConfigurableEntryList.Entry(dgvpanel, "Grid", "", new System.Drawing.Point(3, 30), new System.Drawing.Size(800, 400), null)
            { Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom });
            f.AddOK(new Point(800 - 100, 460), "OK", anchor: AnchorStyles.Right | AnchorStyles.Bottom);
            f.InstallStandardTriggers();
            f.AllowResize = true;

            string title = "Shipyard for " + si.StationName + " " + (si.ShipyardUpdateUTC.Year > 2000 ? " " + EliteConfigInstance.InstanceConfig.ConvertTimeToSelectedFromUTC(si.ShipyardUpdateUTC).ToString() : "No Data");
            f.ShowDialogCentred(fm,fm.Icon, title, closeicon: true);

            saver.DGVSaveColumnLayout(dgvpanel.DataGrid, "ShowShipyard");
        }



    }

}


