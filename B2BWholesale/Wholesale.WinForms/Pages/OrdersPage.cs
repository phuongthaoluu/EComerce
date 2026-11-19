using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Wholesale.WinForms.Data;
using Wholesale.WinForms.Reports;
using Wholesale.WinForms.Services;
using Wholesale.WinForms.UI;

namespace Wholesale.WinForms.Pages
{
    /// <summary>
    /// Task 1.2 (order management): view incoming web orders and update the OrderStatus.
    /// Task 1.3: print / export the Shipping Manifest of the selected order.
    /// </summary>
    public class OrdersPage : PageBase
    {
        private readonly TextBox _txtSearch = new TextBox();
        private readonly ComboBox _cboFilter = new ComboBox();
        private readonly CheckBox _chkAuto = new CheckBox();
        private readonly Label _lblPending = new Label();
        private readonly DataGridView _gridOrders = new DataGridView();
        private readonly DataGridView _gridLines = new DataGridView();
        private readonly Timer _timer = new Timer();

        private readonly Label _lblOrderNo = new Label();
        private readonly Label _lblAgent = new Label();
        private readonly Label _lblAddress = new Label();
        private readonly Label _lblDate = new Label();
        private readonly Label _lblStatus = new Label();
        private readonly Label _lblTotal = new Label();
        private readonly ComboBox _cboNext = new ComboBox();
        private Button _btnUpdate;
        private Button _btnPreview;
        private Button _btnPrint;
        private Button _btnPdf;

        private int _selectedOrderId;
        private string _selectedStatus;
        private bool _loading;

        public override string PageTitle { get { return "Orders"; } }

        public OrdersPage()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
            layout.Controls.Add(BuildFilterCard(), 0, 0);
            layout.Controls.Add(BuildOrdersCard(), 0, 1);
            layout.Controls.Add(BuildDetailCard(), 0, 2);
            Controls.Add(layout);

            _timer.Interval = 15000;
            _timer.Tick += (s, e) => LoadOrders();
        }

        private Control BuildFilterCard()
        {
            Panel card = Theme.MakeCard();
            card.Padding = new Padding(10, 8, 10, 4);
            var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            bar.Controls.Add(new Label { Text = "Search (order # / agent):", AutoSize = true, Margin = new Padding(0, 8, 6, 0), ForeColor = Theme.TextMuted });
            _txtSearch.Width = 200;
            _txtSearch.Margin = new Padding(0, 4, 14, 0);
            _txtSearch.TextChanged += (s, e) => LoadOrders();
            bar.Controls.Add(_txtSearch);

            bar.Controls.Add(new Label { Text = "Status:", AutoSize = true, Margin = new Padding(0, 8, 6, 0), ForeColor = Theme.TextMuted });
            _cboFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboFilter.Width = 130;
            _cboFilter.Margin = new Padding(0, 4, 14, 0);
            _cboFilter.Items.Add("All");
            foreach (string st in OrderRules.AllStatuses) _cboFilter.Items.Add(st);
            _cboFilter.SelectedIndex = 0;
            _cboFilter.SelectedIndexChanged += (s, e) => LoadOrders();
            bar.Controls.Add(_cboFilter);

            Button refresh = Theme.MakeButton("Refresh", Theme.Grey, 90);
            refresh.Margin = new Padding(0, 0, 12, 0);
            refresh.Click += (s, e) => LoadOrders();
            bar.Controls.Add(refresh);

            _chkAuto.Text = "Auto-refresh (15 s)";
            _chkAuto.AutoSize = true;
            _chkAuto.Margin = new Padding(0, 8, 16, 0);
            _chkAuto.CheckedChanged += (s, e) => _timer.Enabled = _chkAuto.Checked && Visible;
            bar.Controls.Add(_chkAuto);

            _lblPending.AutoSize = true;
            _lblPending.Margin = new Padding(0, 8, 0, 0);
            _lblPending.Font = Theme.BoldFont;
            _lblPending.ForeColor = Theme.Warning;
            bar.Controls.Add(_lblPending);

            card.Controls.Add(bar);
            return card;
        }

        private Control BuildOrdersCard()
        {
            Panel card = Theme.MakeCard();
            Theme.StyleGrid(_gridOrders);
            _gridOrders.SelectionChanged += (s, e) => { if (!_loading) OnOrderSelected(); };
            _gridOrders.DataBindingComplete += (s, e) => ColourStatus();
            card.Controls.Add(_gridOrders);
            return card;
        }

        private Control BuildDetailCard()
        {
            Panel card = Theme.MakeCard();
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 370));

            Theme.StyleGrid(_gridLines);
            t.Controls.Add(_gridLines, 0, 0);

            var side = new FlowLayoutPanel
{
    Dock = DockStyle.Fill,
    FlowDirection = FlowDirection.TopDown,
    WrapContents = false,
    AutoScroll = true,
    Padding = new Padding(12, 4, 8, 8)
};
side.Controls.Add(new Label
{
    Text = "Order details",
    Font = Theme.SectionFont,
    ForeColor = Theme.Navy,
    AutoSize = true,
    Margin = new Padding(0, 0, 0, 4)
});

foreach (Label l in new[] { _lblOrderNo, _lblAgent, _lblAddress, _lblDate, _lblStatus, _lblTotal })
{
    l.AutoSize = false;
    l.Width = 335;
    l.Height = 24;
    l.TextAlign = ContentAlignment.MiddleLeft;
    l.ForeColor = Theme.Navy;
    l.Margin = new Padding(0, 1, 0, 1);
    side.Controls.Add(l);
}

_lblAddress.Height = 42;
_lblAddress.TextAlign = ContentAlignment.TopLeft;
_lblTotal.Font = Theme.BoldFont;

Label lblChange = Theme.MakeLabel("Change status to", true);
lblChange.Margin = new Padding(0, 8, 0, 2);
side.Controls.Add(lblChange);

_cboNext.DropDownStyle = ComboBoxStyle.DropDownList;
_cboNext.Width = 220;
_cboNext.Margin = new Padding(0, 0, 0, 6);
side.Controls.Add(_cboNext);

var buttons = new FlowLayoutPanel
{
    Width = 340,
    Height = 96,
    WrapContents = true,
    AutoSize = false,
    Margin = new Padding(0, 4, 0, 0)
};
_btnUpdate = Theme.MakeButton("Update status", Theme.Primary, 135);
_btnPreview = Theme.MakeButton("Manifest preview", Theme.Grey, 135);
_btnPrint = Theme.MakeButton("Print manifest", Theme.Success, 135);
_btnPdf = Theme.MakeButton("Export PDF", Theme.Warning, 135);
_btnUpdate.Click += (s, e) => OnUpdateStatus();
_btnPreview.Click += (s, e) => OnManifest("preview");
_btnPrint.Click += (s, e) => OnManifest("print");
_btnPdf.Click += (s, e) => OnManifest("pdf");
buttons.Controls.AddRange(new Control[] { _btnUpdate, _btnPreview, _btnPrint, _btnPdf });
side.Controls.Add(buttons);

            t.Controls.Add(side, 1, 0);
            card.Controls.Add(t);
            SetDetailEnabled(false);
            return card;
        }

        public override void OnShown()
        {
            LoadOrders();
            _timer.Enabled = _chkAuto.Checked;
        }

        public override void OnHidden() { _timer.Enabled = false; }

        private void SetDetailEnabled(bool on)
        {
            _btnPreview.Enabled = on;
            _btnPrint.Enabled = on;
            _btnPdf.Enabled = on;
            if (!on)
            {
                _btnUpdate.Enabled = false;
                _cboNext.Enabled = false;
                _cboNext.Items.Clear();
                _lblOrderNo.Text = "Select an order above.";
                _lblAgent.Text = ""; _lblAddress.Text = ""; _lblDate.Text = ""; _lblStatus.Text = ""; _lblTotal.Text = "";
                _gridLines.DataSource = null;
            }
        }

        private void LoadOrders()
        {
            try
            {
                _loading = true;
                string status = _cboFilter.SelectedIndex <= 0 ? null : Convert.ToString(_cboFilter.SelectedItem);
                DataTable dt = OrderRepository.GetOrders(status, _txtSearch.Text);
                _gridOrders.DataSource = dt;
                Rename(_gridOrders, "OrderID", "Order #", 35);
                Rename(_gridOrders, "OrderDate", "Order date", 55);
                Rename(_gridOrders, "AgentName", "Agent", 90);
                Rename(_gridOrders, "Phone", "Phone", 45);
                Rename(_gridOrders, "OrderStatus", "Status", 40);
                Rename(_gridOrders, "TotalQty", "Total qty", 35);
                Rename(_gridOrders, "TotalAmount", "Total amount (VND)", 60);
                if (_gridOrders.Columns.Contains("OrderDate")) _gridOrders.Columns["OrderDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                if (_gridOrders.Columns.Contains("TotalAmount"))
                {
                    _gridOrders.Columns["TotalAmount"].DefaultCellStyle.Format = "N0";
                    _gridOrders.Columns["TotalAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                if (_gridOrders.Columns.Contains("TotalQty")) _gridOrders.Columns["TotalQty"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                int pending = OrderRepository.CountByStatus("Pending");
                _lblPending.Text = pending > 0 ? "● " + pending + " new order(s) waiting for confirmation" : "";

                _gridOrders.ClearSelection();
                bool found = false;
                if (_selectedOrderId > 0)
                {
                    foreach (DataGridViewRow r in _gridOrders.Rows)
                    {
                        if (Convert.ToInt32(r.Cells["OrderID"].Value) == _selectedOrderId)
                        {
                            r.Selected = true;
                            _gridOrders.CurrentCell = r.Cells["AgentName"];
                            found = true;
                            break;
                        }
                    }
                }
                _loading = false;
                if (found) OnOrderSelected();
                else { _selectedOrderId = 0; SetDetailEnabled(false); }
            }
            catch (Exception ex)
            {
                _loading = false;
                _timer.Enabled = false;
                Theme.Error(this, Db.Friendly(ex));
            }
        }

        private static void Rename(DataGridView g, string col, string header, float weight)
        {
            if (!g.Columns.Contains(col)) return;
            g.Columns[col].HeaderText = header;
            g.Columns[col].FillWeight = weight;
        }

        private void ColourStatus()
        {
            foreach (DataGridViewRow r in _gridOrders.Rows)
            {
                string st = Convert.ToString(r.Cells["OrderStatus"].Value);
                Color c = Theme.Navy;
                switch (st)
                {
                    case "Pending": c = Theme.Warning; break;
                    case "Confirmed": c = Theme.Primary; break;
                    case "Shipped": c = Color.FromArgb(124, 58, 237); break;
                    case "Delivered": c = Theme.Success; break;
                    case "Cancelled": c = Theme.Danger; break;
                }
                r.Cells["OrderStatus"].Style.ForeColor = c;
                r.Cells["OrderStatus"].Style.Font = Theme.BoldFont;
            }
        }

        private void OnOrderSelected()
        {
            if (_gridOrders.SelectedRows.Count == 0) return;
            DataGridViewRow row = _gridOrders.SelectedRows[0];
            _selectedOrderId = Convert.ToInt32(row.Cells["OrderID"].Value);
            try
            {
                DataRow h = OrderRepository.GetHeader(_selectedOrderId);
                if (h == null) { SetDetailEnabled(false); return; }
                _selectedStatus = Convert.ToString(h["OrderStatus"]);
                _lblOrderNo.Text = "Order #" + _selectedOrderId;
                _lblAgent.Text = "Agent: " + Convert.ToString(h["AgentName"]) + "  (" + Convert.ToString(h["Phone"]) + ")";
                _lblAddress.Text = "Address: " + Convert.ToString(h["Address"]);
                _lblDate.Text = "Date: " + Convert.ToDateTime(h["OrderDate"]).ToString("dd/MM/yyyy HH:mm");
                _lblStatus.Text = "Status: " + _selectedStatus;

                DataTable lines = OrderRepository.GetLines(_selectedOrderId);
                _gridLines.DataSource = lines;
                Rename(_gridLines, "ID", "Line", 20);
                Rename(_gridLines, "ItemName", "Item", 100);
                Rename(_gridLines, "Size", "Size", 70);
                Rename(_gridLines, "Quantity", "Qty", 30);
                Rename(_gridLines, "UnitAmount", "Unit price", 50);
                Rename(_gridLines, "LineTotal", "Line total", 55);
                foreach (string c in new[] { "UnitAmount", "LineTotal", "Quantity" })
                {
                    if (!_gridLines.Columns.Contains(c)) continue;
                    _gridLines.Columns[c].DefaultCellStyle.Format = "N0";
                    _gridLines.Columns[c].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                decimal total = 0;
                foreach (DataRow lr in lines.Rows) total += Convert.ToDecimal(lr["LineTotal"]);
                _lblTotal.Text = "Total: " + Theme.Money(total);

                IList<string> next = OrderRules.NextStatuses(_selectedStatus);
                _cboNext.Items.Clear();
                foreach (string n in next) _cboNext.Items.Add(n);
                if (_cboNext.Items.Count > 0) _cboNext.SelectedIndex = 0;
                _cboNext.Enabled = next.Count > 0;
                _btnUpdate.Enabled = next.Count > 0;
                _btnPreview.Enabled = true;
                _btnPrint.Enabled = true;
                _btnPdf.Enabled = true;
            }
            catch (Exception ex) { Theme.Error(this, Db.Friendly(ex)); }
        }

        private void OnUpdateStatus()
        {
            if (_selectedOrderId == 0 || _cboNext.SelectedItem == null) return;
            string target = Convert.ToString(_cboNext.SelectedItem);
            string msg = "Change order #" + _selectedOrderId + " from '" + _selectedStatus + "' to '" + target + "'?";
            if (target == "Cancelled") msg += "\n\nThe ordered quantities will be returned to stock.";
            if (!Theme.Confirm(this, msg)) return;
            try
            {
                OrderRepository.UpdateStatus(_selectedOrderId, target);
                LoadOrders();
                Theme.Info(this, "Order #" + _selectedOrderId + " is now '" + target + "'.");
            }
            catch (Exception ex)
            {
                Theme.Error(this, Db.Friendly(ex));
                LoadOrders();
            }
        }

        private void OnManifest(string action)
        {
            if (_selectedOrderId == 0) return;
            try
            {
                var doc = new ManifestDocument(ManifestData.Load(_selectedOrderId));
                if (action == "preview") ManifestExporter.Preview(this, doc);
                else if (action == "print") ManifestExporter.PrintWithDialog(this, doc);
                else
                {
                    using (var dlg = new SaveFileDialog())
                    {
                        dlg.Filter = "PDF file (*.pdf)|*.pdf";
                        dlg.FileName = "ShippingManifest_Order" + _selectedOrderId + ".pdf";
                        if (dlg.ShowDialog(this) != DialogResult.OK) return;
                        string error;
                        if (ManifestExporter.TryExportPdf(doc, dlg.FileName, out error))
                            Theme.Info(this, "Shipping manifest saved:\n" + dlg.FileName);
                        else
                            Theme.Warn(this, "PDF export failed.\n\n" + error);
                    }
                }
            }
            catch (Exception ex) { Theme.Error(this, Db.Friendly(ex)); }
        }
    }
}
