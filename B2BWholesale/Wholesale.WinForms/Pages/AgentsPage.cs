using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Wholesale.WinForms.Data;
using Wholesale.WinForms.UI;

namespace Wholesale.WinForms.Pages
{
    /// <summary>Task 1.2 - CRUD for distributor Agents.</summary>
    public class AgentsPage : PageBase
    {
        private readonly TextBox _txtSearch = new TextBox();
        private readonly DataGridView _grid = new DataGridView();
        private readonly Label _lblCount = new Label();
        private readonly TextBox _txtId = new TextBox();
        private readonly TextBox _txtName = new TextBox();
        private readonly TextBox _txtAddress = new TextBox();
        private readonly TextBox _txtPhone = new TextBox();

        private int _selectedId;
        private bool _loading;

        public override string PageTitle { get { return "Agents"; } }

        public AgentsPage()
        {
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
            bar.Controls.Add(new Label { Text = "Search:", AutoSize = true, Margin = new Padding(0, 8, 6, 0), ForeColor = Theme.TextMuted });
            _txtSearch.Width = 300;
            _txtSearch.Margin = new Padding(0, 4, 12, 0);
            _txtSearch.TextChanged += (s, e) => LoadData();
            bar.Controls.Add(_txtSearch);
            Button refresh = Theme.MakeButton("Refresh", Theme.Grey, 90);
            refresh.Margin = new Padding(0, 0, 12, 0);
            refresh.Click += (s, e) => LoadData();
            bar.Controls.Add(refresh);
            _lblCount.AutoSize = true;
            _lblCount.Margin = new Padding(0, 8, 0, 0);
            _lblCount.ForeColor = Theme.TextMuted;
            bar.Controls.Add(_lblCount);

            Theme.StyleGrid(_grid);
            _grid.SelectionChanged += (s, e) => { if (!_loading) FillFormFromSelection(); };

            t.Controls.Add(bar, 0, 0);
            t.Controls.Add(_grid, 0, 1);
            card.Controls.Add(t);
            return card;
        }

        private Control BuildEditCard()
        {
            Panel card = Theme.MakeCard();
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            flow.Controls.Add(new Label { Text = "Agent details", Font = Theme.SectionFont, ForeColor = Theme.Navy, AutoSize = true, Margin = new Padding(0, 0, 0, 4) });

            flow.Controls.Add(Theme.MakeLabel("Agent ID", false));
            _txtId.Width = 300; _txtId.ReadOnly = true; _txtId.BackColor = Color.FromArgb(243, 244, 246);
            flow.Controls.Add(_txtId);

            flow.Controls.Add(Theme.MakeLabel("Agent name *", false));
            _txtName.Width = 300; _txtName.MaxLength = 150;
            flow.Controls.Add(_txtName);

            flow.Controls.Add(Theme.MakeLabel("Address *", false));
            _txtAddress.Width = 300; _txtAddress.Height = 70; _txtAddress.Multiline = true; _txtAddress.MaxLength = 250;
            flow.Controls.Add(_txtAddress);

            flow.Controls.Add(Theme.MakeLabel("Phone * (digits, 8-15)", false));
            _txtPhone.Width = 300; _txtPhone.MaxLength = 15;
            flow.Controls.Add(_txtPhone);

            var row = new FlowLayoutPanel { Width = 310, Height = 90, WrapContents = true, Margin = new Padding(0, 12, 0, 0) };
            Button add = Theme.MakeButton("Add new", Theme.Success, 96);
            Button upd = Theme.MakeButton("Update", Theme.Primary, 96);
            Button del = Theme.MakeButton("Delete", Theme.Danger, 96);
            Button clr = Theme.MakeButton("Clear form", Theme.Grey, 96);
            add.Click += (s, e) => OnAdd();
            upd.Click += (s, e) => OnUpdate();
            del.Click += (s, e) => OnDelete();
            clr.Click += (s, e) => { _grid.ClearSelection(); ClearForm(); };
            row.Controls.AddRange(new Control[] { add, upd, del, clr });
            flow.Controls.Add(row);

            flow.Controls.Add(new Label
            {
                Text = "An agent that already has orders cannot be deleted (the order history must be kept). Deleting an agent without orders also removes its web login.",
                ForeColor = Theme.TextMuted, Width = 300, Height = 70, Margin = new Padding(0, 8, 0, 0)
            });
            card.Controls.Add(flow);
            return card;
        }

        public override void OnShown() { LoadData(); }

        private void LoadData()
        {
            try
            {
                _loading = true;
                DataTable dt = AgentRepository.Search(_txtSearch.Text);
                _grid.DataSource = dt;
                if (_grid.Columns.Contains("AgentID")) { _grid.Columns["AgentID"].HeaderText = "ID"; _grid.Columns["AgentID"].FillWeight = 25; }
                if (_grid.Columns.Contains("AgentName")) { _grid.Columns["AgentName"].HeaderText = "Agent name"; _grid.Columns["AgentName"].FillWeight = 90; }
                if (_grid.Columns.Contains("Address")) _grid.Columns["Address"].FillWeight = 170;
                if (_grid.Columns.Contains("Phone")) _grid.Columns["Phone"].FillWeight = 55;
                if (_grid.Columns.Contains("Orders")) { _grid.Columns["Orders"].FillWeight = 30; _grid.Columns["Orders"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; }
                _lblCount.Text = dt.Rows.Count + " agent(s)";

                _grid.ClearSelection();
                bool found = false;
                if (_selectedId > 0)
                {
                    foreach (DataGridViewRow r in _grid.Rows)
                    {
                        if (Convert.ToInt32(r.Cells["AgentID"].Value) == _selectedId)
                        {
                            r.Selected = true;
                            _grid.CurrentCell = r.Cells["AgentName"];
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

        private void FillFormFromSelection()
        {
            if (_grid.SelectedRows.Count == 0) return;
            DataGridViewRow r = _grid.SelectedRows[0];
            _selectedId = Convert.ToInt32(r.Cells["AgentID"].Value);
            _txtId.Text = _selectedId.ToString();
            _txtName.Text = Convert.ToString(r.Cells["AgentName"].Value);
            _txtAddress.Text = Convert.ToString(r.Cells["Address"].Value);
            _txtPhone.Text = Convert.ToString(r.Cells["Phone"].Value);
        }

        private void ClearForm()
        {
            _selectedId = 0;
            _txtId.Text = "";
            _txtName.Text = "";
            _txtAddress.Text = "";
            _txtPhone.Text = "";
            _txtName.Focus();
        }

        private bool ValidateInput(out string name, out string address, out string phone)
        {
            name = _txtName.Text.Trim();
            address = _txtAddress.Text.Trim();
            phone = _txtPhone.Text.Trim();
            if (name.Length == 0) { Theme.Warn(this, "Agent name is required."); _txtName.Focus(); return false; }
            if (address.Length == 0) { Theme.Warn(this, "Address is required."); _txtAddress.Focus(); return false; }
            if (!Regex.IsMatch(phone, @"^\+?[0-9]{8,14}$")) { Theme.Warn(this, "Phone must contain 8 to 15 digits (optional leading +)."); _txtPhone.Focus(); return false; }
            return true;
        }

        private void OnAdd()
        {
            string n, a, p;
            if (!ValidateInput(out n, out a, out p)) return;
            try
            {
                _selectedId = AgentRepository.Insert(n, a, p);
                LoadData();
                Theme.Info(this, "Agent added (ID " + _selectedId + ").");
            }
            catch (Exception ex) { Theme.Error(this, Db.Friendly(ex)); }
        }

        private void OnUpdate()
        {
            if (_selectedId == 0) { Theme.Warn(this, "Select an agent in the list first."); return; }
            string n, a, p;
            if (!ValidateInput(out n, out a, out p)) return;
            try
            {
                AgentRepository.Update(_selectedId, n, a, p);
                LoadData();
                Theme.Info(this, "Agent updated.");
            }
            catch (Exception ex) { Theme.Error(this, Db.Friendly(ex)); }
        }

        private void OnDelete()
        {
            if (_selectedId == 0) { Theme.Warn(this, "Select an agent in the list first."); return; }
            if (!Theme.Confirm(this, "Delete agent '" + _txtName.Text + "'?")) return;
            try
            {
                AgentRepository.Delete(_selectedId);
                _selectedId = 0;
                LoadData();
                Theme.Info(this, "Agent deleted.");
            }
            catch (Exception ex)
            {
                Theme.Error(this, "The agent could not be deleted (an agent with orders must be kept).\n\n" + Db.Friendly(ex));
            }
        }
    }
}
