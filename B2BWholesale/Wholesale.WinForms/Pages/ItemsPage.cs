using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Wholesale.WinForms.Data;
using Wholesale.WinForms.UI;

namespace Wholesale.WinForms.Pages
{
    /// <summary>Task 1.2 - CRUD for Items, including stock updates.</summary>
    public class ItemsPage : PageBase
    {
        private readonly TextBox _txtSearch = new TextBox();
        private readonly CheckBox _chkLow = new CheckBox();
        private readonly DataGridView _grid = new DataGridView();
        private readonly Label _lblCount = new Label();

        private readonly TextBox _txtId = new TextBox();
        private readonly TextBox _txtName = new TextBox();
        private readonly TextBox _txtSize = new TextBox();
        private readonly NumericUpDown _numPrice = new NumericUpDown();
        private readonly NumericUpDown _numStock = new NumericUpDown();
        private readonly NumericUpDown _numAdjust = new NumericUpDown();

        private int _selectedId;      // 0 = nothing selected (add mode)
        private bool _loading;
        private int _threshold;

        public override string PageTitle { get { return "Items & Stock"; } }

        public ItemsPage()
        {
            _threshold = Db.GetIntSetting("LowStockThreshold", 20);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
            layout.Controls.Add(BuildListCard(), 0, 0);
            layout.Controls.Add(BuildEditCard(), 1, 0);
            Controls.Add(layout);
        }

        private Control BuildListCard()
        {
            Panel card = Theme.MakeCard();
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            var lbl = new Label { Text = "Search:", AutoSize = true, Margin = new Padding(0, 8, 6, 0), ForeColor = Theme.TextMuted };
            _txtSearch.Width = 260;
            _txtSearch.Margin = new Padding(0, 4, 12, 0);
            _txtSearch.TextChanged += (s, e) => LoadData();
            _chkLow.Text = "Low stock only (<= " + _threshold + ")";
            _chkLow.AutoSize = true;
            _chkLow.Margin = new Padding(0, 7, 12, 0);
            _chkLow.CheckedChanged += (s, e) => LoadData();
            Button refresh = Theme.MakeButton("Refresh", Theme.Grey, 90);
            refresh.Margin = new Padding(0, 0, 12, 0);
            refresh.Click += (s, e) => LoadData();
            _lblCount.AutoSize = true;
            _lblCount.Margin = new Padding(0, 8, 0, 0);
            _lblCount.ForeColor = Theme.TextMuted;
            bar.Controls.AddRange(new Control[] { lbl, _txtSearch, _chkLow, refresh, _lblCount });

            Theme.StyleGrid(_grid);
            _grid.SelectionChanged += (s, e) => { if (!_loading) FillFormFromSelection(); };
            _grid.DataBindingComplete += (s, e) => ColourRows();

            t.Controls.Add(bar, 0, 0);
            t.Controls.Add(_grid, 0, 1);
            card.Controls.Add(t);
            return card;
        }

        private Control BuildEditCard()
        {
            Panel card = Theme.MakeCard();
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };

            flow.Controls.Add(new Label { Text = "Item details", Font = Theme.SectionFont, ForeColor = Theme.Navy, AutoSize = true, Margin = new Padding(0, 0, 0, 4) });

            flow.Controls.Add(Theme.MakeLabel("Item ID", false));
            _txtId.Width = 300; _txtId.ReadOnly = true; _txtId.BackColor = Color.FromArgb(243, 244, 246);
            flow.Controls.Add(_txtId);

            flow.Controls.Add(Theme.MakeLabel("Item name *", false));
            _txtName.Width = 300; _txtName.MaxLength = 150;
            flow.Controls.Add(_txtName);

            flow.Controls.Add(Theme.MakeLabel("Size / packing *", false));
            _txtSize.Width = 300; _txtSize.MaxLength = 50;
            flow.Controls.Add(_txtSize);

            flow.Controls.Add(Theme.MakeLabel("Price (VND) *", false));
            _numPrice.Width = 300; _numPrice.DecimalPlaces = 0; _numPrice.Minimum = 0; _numPrice.Maximum = 1000000000; _numPrice.Increment = 1000; _numPrice.ThousandsSeparator = true;
            flow.Controls.Add(_numPrice);

            flow.Controls.Add(Theme.MakeLabel("Stock quantity *", false));
            _numStock.Width = 300; _numStock.Minimum = 0; _numStock.Maximum = 10000000; _numStock.ThousandsSeparator = true;
            flow.Controls.Add(_numStock);

            var row1 = new FlowLayoutPanel { Width = 310, Height = 90, WrapContents = true, Margin = new Padding(0, 12, 0, 0) };
            Button add = Theme.MakeButton("Add new", Theme.Success, 96);
            Button upd = Theme.MakeButton("Update", Theme.Primary, 96);
            Button del = Theme.MakeButton("Delete", Theme.Danger, 96);
            Button clr = Theme.MakeButton("Clear form", Theme.Grey, 96);
            add.Click += (s, e) => OnAdd();
            upd.Click += (s, e) => OnUpdate();
            del.Click += (s, e) => OnDelete();
            clr.Click += (s, e) => { _grid.ClearSelection(); ClearForm(); };
            row1.Controls.AddRange(new Control[] { add, upd, del, clr });
            flow.Controls.Add(row1);

            flow.Controls.Add(new Label { Text = "Quick stock update", Font = Theme.SectionFont, ForeColor = Theme.Navy, AutoSize = true, Margin = new Padding(0, 10, 0, 4) });
            flow.Controls.Add(Theme.MakeLabel("Quantity to add / remove for the selected item", false));
            _numAdjust.Width = 300; _numAdjust.Minimum = 1; _numAdjust.Maximum = 1000000; _numAdjust.Value = 10; _numAdjust.ThousandsSeparator = true;
            flow.Controls.Add(_numAdjust);
            var row2 = new FlowLayoutPanel { Width = 310, Height = 44, WrapContents = false, Margin = new Padding(0, 6, 0, 0) };
            Button recv = Theme.MakeButton("+ Receive stock", Theme.Success, 146);
            Button issue = Theme.MakeButton("- Issue stock", Theme.Warning, 146);
            recv.Click += (s, e) => OnAdjust(1);
            issue.Click += (s, e) => OnAdjust(-1);
            row2.Controls.AddRange(new Control[] { recv, issue });
            flow.Controls.Add(row2);

            card.Controls.Add(flow);
            return card;
        }

        public override void OnShown() { LoadData(); }

        private void LoadData()
        {
            try
            {
                _loading = true;
                DataTable dt = ItemRepository.Search(_txtSearch.Text, _chkLow.Checked, _threshold);
                _grid.DataSource = dt;
                if (_grid.Columns.Contains("ItemID")) { _grid.Columns["ItemID"].HeaderText = "ID"; _grid.Columns["ItemID"].FillWeight = 30; }
                if (_grid.Columns.Contains("ItemName")) { _grid.Columns["ItemName"].HeaderText = "Item name"; _grid.Columns["ItemName"].FillWeight = 120; }
                if (_grid.Columns.Contains("Size")) _grid.Columns["Size"].FillWeight = 80;
                if (_grid.Columns.Contains("Price"))
                {
                    _grid.Columns["Price"].HeaderText = "Price (VND)";
                    _grid.Columns["Price"].DefaultCellStyle.Format = "N0";
                    _grid.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    _grid.Columns["Price"].FillWeight = 60;
                }
                if (_grid.Columns.Contains("StockQuantity"))
                {
                    _grid.Columns["StockQuantity"].HeaderText = "Stock";
                    _grid.Columns["StockQuantity"].DefaultCellStyle.Format = "N0";
                    _grid.Columns["StockQuantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    _grid.Columns["StockQuantity"].FillWeight = 45;
                }
                _lblCount.Text = dt.Rows.Count + " item(s)";

                _grid.ClearSelection();
                bool found = false;
                if (_selectedId > 0)
                {
                    foreach (DataGridViewRow r in _grid.Rows)
                    {
                        if (Convert.ToInt32(r.Cells["ItemID"].Value) == _selectedId)
                        {
                            r.Selected = true;
                            _grid.CurrentCell = r.Cells["ItemName"];
                            found = true;
                            break;
                        }
                    }
                }
                _loading = false;
                if (found) FillFormFromSelection(); else ClearForm();
            }
            catch (Exception ex)
            {
                _loading = false;
                Theme.Error(this, Db.Friendly(ex));
            }
        }

        private void ColourRows()
        {
            foreach (DataGridViewRow r in _grid.Rows)
            {
                int stock = Convert.ToInt32(r.Cells["StockQuantity"].Value);
                if (stock == 0) r.DefaultCellStyle.ForeColor = Theme.Danger;
                else if (stock <= _threshold) r.DefaultCellStyle.ForeColor = Theme.Warning;
                else r.DefaultCellStyle.ForeColor = Theme.Navy;
            }
        }

        private void FillFormFromSelection()
        {
            if (_grid.SelectedRows.Count == 0) return;
            DataGridViewRow r = _grid.SelectedRows[0];
            _selectedId = Convert.ToInt32(r.Cells["ItemID"].Value);
            _txtId.Text = _selectedId.ToString();
            _txtName.Text = Convert.ToString(r.Cells["ItemName"].Value);
            _txtSize.Text = Convert.ToString(r.Cells["Size"].Value);
            _numPrice.Value = Math.Min(_numPrice.Maximum, Convert.ToDecimal(r.Cells["Price"].Value));
            _numStock.Value = Math.Min(_numStock.Maximum, Convert.ToDecimal(r.Cells["StockQuantity"].Value));
        }

        private void ClearForm()
        {
            _selectedId = 0;
            _txtId.Text = "";
            _txtName.Text = "";
            _txtSize.Text = "";
            _numPrice.Value = 0;
            _numStock.Value = 0;
            _txtName.Focus();
        }

        private bool ValidateInput(out string name, out string size)
        {
            name = _txtName.Text.Trim();
            size = _txtSize.Text.Trim();
            if (name.Length == 0) { Theme.Warn(this, "Item name is required."); _txtName.Focus(); return false; }
            if (size.Length == 0) { Theme.Warn(this, "Size / packing is required."); _txtSize.Focus(); return false; }
            if (_numPrice.Value <= 0) { Theme.Warn(this, "Price must be greater than 0."); _numPrice.Focus(); return false; }
            return true;
        }

        private void OnAdd()
        {
            string name, size;
            if (!ValidateInput(out name, out size)) return;
            try
            {
                _selectedId = ItemRepository.Insert(name, size, _numPrice.Value, (int)_numStock.Value);
                LoadData();
                Theme.Info(this, "Item added (ID " + _selectedId + ").");
            }
            catch (Exception ex) { Theme.Error(this, Db.Friendly(ex)); }
        }

        private void OnUpdate()
        {
            if (_selectedId == 0) { Theme.Warn(this, "Select an item in the list first."); return; }
            string name, size;
            if (!ValidateInput(out name, out size)) return;
            try
            {
                ItemRepository.Update(_selectedId, name, size, _numPrice.Value, (int)_numStock.Value);
                LoadData();
                Theme.Info(this, "Item updated.");
            }
            catch (Exception ex) { Theme.Error(this, Db.Friendly(ex)); }
        }

        private void OnDelete()
        {
            if (_selectedId == 0) { Theme.Warn(this, "Select an item in the list first."); return; }
            if (!Theme.Confirm(this, "Delete item '" + _txtName.Text + "' (" + _txtSize.Text + ")?")) return;
            try
            {
                ItemRepository.Delete(_selectedId);
                _selectedId = 0;
                LoadData();
                Theme.Info(this, "Item deleted.");
            }
            catch (Exception ex)
            {
                Theme.Error(this, "The item could not be deleted.\n\n" + Db.Friendly(ex));
            }
        }

        private void OnAdjust(int sign)
        {
            if (_selectedId == 0) { Theme.Warn(this, "Select an item in the list first."); return; }
            int delta = sign * (int)_numAdjust.Value;
            try
            {
                if (!ItemRepository.AdjustStock(_selectedId, delta))
                {
                    Theme.Warn(this, "Not enough stock: you cannot issue more than the quantity on hand.");
                    return;
                }
                LoadData();
            }
            catch (Exception ex) { Theme.Error(this, Db.Friendly(ex)); }
        }
    }
}
