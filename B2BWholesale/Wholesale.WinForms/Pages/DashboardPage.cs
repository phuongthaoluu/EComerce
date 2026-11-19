using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Wholesale.WinForms.Data;
using Wholesale.WinForms.UI;

namespace Wholesale.WinForms.Pages
{
    /// <summary>
    /// Task 1.4 - advanced data filter / dashboard:
    /// best-selling items, low-stock warning and total revenue by (specific) agent.
    /// </summary>
    public class DashboardPage : PageBase
    {
        private readonly DateTimePicker _dtFrom = new DateTimePicker();
        private readonly DateTimePicker _dtTo = new DateTimePicker();
        private readonly ComboBox _cboAgent = new ComboBox();
        private readonly ComboBox _cboBasis = new ComboBox();
        private readonly NumericUpDown _numTop = new NumericUpDown();
        private readonly NumericUpDown _numThreshold = new NumericUpDown();

        private readonly Label _kpiRevenue = new Label();
        private readonly Label _kpiOrders = new Label();
        private readonly Label _kpiLow = new Label();
        private readonly Label _kpiTop = new Label();

        private readonly DataGridView _gridBest = new DataGridView();
        private readonly DataGridView _gridLow = new DataGridView();
        private readonly DataGridView _gridRevenue = new DataGridView();
        private readonly BarChartControl _chart = new BarChartControl();

        private bool _initialised;

        public override string PageTitle { get { return "Dashboard"; } }

        public DashboardPage()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(BuildFilterCard(), 0, 0);
            root.Controls.Add(BuildKpiRow(), 0, 1);
            root.Controls.Add(BuildContent(), 0, 2);
            Controls.Add(root);
        }

        // ------------------------------------------------------------------ layout
        private Control BuildFilterCard()
        {
            Panel card = Theme.MakeCard();
            card.Padding = new Padding(10, 6, 10, 2);
            var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = false };

            AddField(bar, "From", _dtFrom, 110);
            AddField(bar, "To", _dtTo, 110);
            _dtFrom.Format = DateTimePickerFormat.Short;
            _dtTo.Format = DateTimePickerFormat.Short;

            _cboAgent.DropDownStyle = ComboBoxStyle.DropDownList;
            AddField(bar, "Agent", _cboAgent, 210);

            _cboBasis.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboBasis.Items.Add("All except Cancelled");
            _cboBasis.Items.Add("Shipped + Delivered only");
            _cboBasis.SelectedIndex = 0;
            AddField(bar, "Orders counted", _cboBasis, 170);

            _numTop.Minimum = 1; _numTop.Maximum = 50; _numTop.Value = 5;
            AddField(bar, "Top N items", _numTop, 60);

            _numThreshold.Minimum = 0; _numThreshold.Maximum = 100000;
            _numThreshold.Value = Db.GetIntSetting("LowStockThreshold", 20);
            AddField(bar, "Low stock ≤", _numThreshold, 70);

            Button apply = Theme.MakeButton("Apply filter", Theme.Primary, 110);
            apply.Margin = new Padding(8, 18, 6, 0);
            apply.Click += (s, e) => LoadDashboard();
            Button reset = Theme.MakeButton("Reset", Theme.Grey, 80);
            reset.Margin = new Padding(0, 18, 0, 0);
            reset.Click += (s, e) => { SetDefaults(); LoadDashboard(); };
            bar.Controls.Add(apply);
            bar.Controls.Add(reset);

            card.Controls.Add(bar);
            return card;
        }

        private static void AddField(FlowLayoutPanel bar, string caption, Control input, int width)
        {
            var box = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Margin = new Padding(0, 0, 12, 0) };
            box.Controls.Add(new Label { Text = caption, AutoSize = true, ForeColor = Theme.TextMuted, Margin = new Padding(0, 0, 0, 2) });
            input.Width = width;
            input.Margin = new Padding(0);
            box.Controls.Add(input);
            bar.Controls.Add(box);
        }

        private Control BuildKpiRow()
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            for (int i = 0; i < 4; i++) t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            t.Controls.Add(MakeKpi("TOTAL REVENUE", _kpiRevenue, Theme.Primary), 0, 0);
            t.Controls.Add(MakeKpi("ORDERS COUNTED", _kpiOrders, Theme.Success), 1, 0);
            t.Controls.Add(MakeKpi("LOW-STOCK ITEMS", _kpiLow, Theme.Danger), 2, 0);
            t.Controls.Add(MakeKpi("BEST-SELLING ITEM", _kpiTop, Theme.Warning), 3, 0);
            return t;
        }

        private static Control MakeKpi(string title, Label value, Color accent)
        {
            Panel card = Theme.MakeCard();
            card.Padding = new Padding(16, 10, 8, 6);
            var bar = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accent };
            var cap = new Label { Text = title, ForeColor = Theme.TextMuted, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), AutoSize = true, Left = 18, Top = 10 };
            value.AutoSize = false;
            value.Left = 18;
            value.Top = 32;
            value.Width = 320;
            value.Height = 40;
            value.Font = Theme.KpiFont;
            value.ForeColor = Theme.Navy;
            value.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            card.Controls.Add(value);
            card.Controls.Add(cap);
            card.Controls.Add(bar);
            return card;
        }

        private Control BuildContent()
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            Theme.StyleGrid(_gridBest);
            Theme.StyleGrid(_gridLow);
            Theme.StyleGrid(_gridRevenue);
            _gridLow.DataBindingComplete += (s, e) => ColourLowStock();

            _chart.Dock = DockStyle.Fill;

            t.Controls.Add(Section("Best-selling items", _gridBest, Theme.Success), 0, 0);
            t.Controls.Add(Section("Low-stock warning", _gridLow, Theme.Danger), 1, 0);
            t.Controls.Add(Section("Total revenue by agent", _gridRevenue, Theme.Primary), 0, 1);
            t.Controls.Add(Section("Revenue by agent (million VND, top 10)", _chart, Theme.Primary), 1, 1);
            return t;
        }

        private static Control Section(string title, Control body, Color accent)
        {
            Panel card = Theme.MakeCard();
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            t.Controls.Add(new Label { Text = title, Font = Theme.SectionFont, ForeColor = accent, AutoSize = true, Margin = new Padding(0, 0, 0, 4) }, 0, 0);
            body.Dock = DockStyle.Fill;
            t.Controls.Add(body, 0, 1);
            card.Controls.Add(t);
            return card;
        }

        // ------------------------------------------------------------------ data
        public override void OnShown()
        {
            try
            {
                DataTable agents = AgentRepository.All();
                DataRow all = agents.NewRow();
                all["AgentID"] = DBNull.Value;
                all["AgentName"] = "(All agents)";
                agents.Rows.InsertAt(all, 0);
                object keep = _cboAgent.SelectedValue;
                _cboAgent.DataSource = agents;
                _cboAgent.DisplayMember = "AgentName";
                _cboAgent.ValueMember = "AgentID";
                if (keep != null && !(keep is DBNull)) _cboAgent.SelectedValue = keep; else _cboAgent.SelectedIndex = 0;

                if (!_initialised) { SetDefaults(); _initialised = true; }
                LoadDashboard();
            }
            catch (Exception ex) { Theme.Error(this, Db.Friendly(ex)); }
        }

        private void SetDefaults()
        {
            DateTime? first = DashboardRepository.FirstOrderDate();
            _dtFrom.Value = first.HasValue ? first.Value.Date : DateTime.Today.AddDays(-90);
            _dtTo.Value = DateTime.Today;
            if (_cboAgent.Items.Count > 0) _cboAgent.SelectedIndex = 0;
            _cboBasis.SelectedIndex = 0;
            _numTop.Value = 5;
            _numThreshold.Value = Db.GetIntSetting("LowStockThreshold", 20);
        }

        private int? SelectedAgentId()
        {
            object v = _cboAgent.SelectedValue;
            if (v == null || v is DBNull) return null;
            return Convert.ToInt32(v);
        }

        private void LoadDashboard()
        {
            if (_dtFrom.Value.Date > _dtTo.Value.Date)
            {
                Theme.Warn(this, "The 'From' date must not be after the 'To' date.");
                return;
            }
            try
            {
                Cursor = Cursors.WaitCursor;
                DateTime from = _dtFrom.Value.Date;
                DateTime to = _dtTo.Value.Date;
                int? agent = SelectedAgentId();
                string basis = _cboBasis.SelectedIndex == 1 ? "COMPLETED" : "ACTIVE";
                int threshold = (int)_numThreshold.Value;

                DataTable best = DashboardRepository.BestSelling(from, to, agent, basis, (int)_numTop.Value);
                DataTable low = DashboardRepository.LowStock(threshold);
                DataTable rev = DashboardRepository.RevenueByAgent(from, to, agent, basis);

                _gridBest.DataSource = best;
                Fmt(_gridBest, "ItemID", "ID", 20, null);
                Fmt(_gridBest, "ItemName", "Item", 100, null);
                Fmt(_gridBest, "Size", "Size", 70, null);
                Fmt(_gridBest, "QtySold", "Qty sold", 40, "N0");
                Fmt(_gridBest, "Revenue", "Revenue (VND)", 60, "N0");

                _gridLow.DataSource = low;
                Fmt(_gridLow, "ItemID", "ID", 20, null);
                Fmt(_gridLow, "ItemName", "Item", 100, null);
                Fmt(_gridLow, "Size", "Size", 70, null);
                Fmt(_gridLow, "StockQuantity", "In stock", 40, "N0");
                Fmt(_gridLow, "Level", "Level", 55, null);

                _gridRevenue.DataSource = rev;
                Fmt(_gridRevenue, "AgentID", "ID", 20, null);
                Fmt(_gridRevenue, "AgentName", "Agent", 100, null);
                Fmt(_gridRevenue, "Orders", "Orders", 30, "N0");
                Fmt(_gridRevenue, "Quantity", "Quantity", 40, "N0");
                Fmt(_gridRevenue, "Revenue", "Revenue (VND)", 60, "N0");

                decimal totalRevenue = 0;
                int orders = 0;
                foreach (DataRow r in rev.Rows)
                {
                    totalRevenue += Convert.ToDecimal(r["Revenue"]);
                    orders += Convert.ToInt32(r["Orders"]);
                }
                _kpiRevenue.Text = Theme.Money(totalRevenue);
                _kpiOrders.Text = orders.ToString("N0");
                _kpiLow.Text = low.Rows.Count.ToString("N0");
                _kpiTop.Text = best.Rows.Count > 0 ? Convert.ToString(best.Rows[0]["ItemName"]) : "-";
                _kpiTop.Font = new Font("Segoe UI", best.Rows.Count > 0 && Convert.ToString(best.Rows[0]["ItemName"]).Length > 18 ? 12f : 18f, FontStyle.Bold);

                var names = new System.Collections.Generic.List<string>();
                var values = new System.Collections.Generic.List<double>();
                foreach (DataRow r in rev.Rows)
                {
                    if (names.Count >= 10) break;
                    names.Add(Convert.ToString(r["AgentName"]));
                    values.Add(Math.Round(Convert.ToDouble(r["Revenue"]) / 1000000.0, 2));
                }
                _chart.SetData(names, values);
            }
            catch (Exception ex) { Theme.Error(this, Db.Friendly(ex)); }
            finally { Cursor = Cursors.Default; }
        }

        private static void Fmt(DataGridView g, string col, string header, float weight, string format)
        {
            if (!g.Columns.Contains(col)) return;
            DataGridViewColumn c = g.Columns[col];
            c.HeaderText = header;
            c.FillWeight = weight;
            if (format != null)
            {
                c.DefaultCellStyle.Format = format;
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void ColourLowStock()
        {
            foreach (DataGridViewRow r in _gridLow.Rows)
            {
                string level = Convert.ToString(r.Cells["Level"].Value);
                Color c = Theme.Warning;
                if (level == "OUT OF STOCK") c = Theme.Danger;
                else if (level == "CRITICAL") c = Color.FromArgb(234, 88, 12);
                r.DefaultCellStyle.ForeColor = c;
                r.Cells["Level"].Style.Font = Theme.BoldFont;
            }
        }
    }
}
