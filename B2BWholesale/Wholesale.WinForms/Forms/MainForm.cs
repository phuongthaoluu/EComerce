using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Wholesale.WinForms.Data;
using Wholesale.WinForms.Security;
using Wholesale.WinForms.UI;

namespace Wholesale.WinForms.Forms
{
    public class MainForm : Form
    {
        private readonly Panel _host = new Panel();
        private readonly Label _lblTitle = new Label();
        private readonly Label _lblUser = new Label();
        private readonly List<Button> _navButtons = new List<Button>();
        private readonly Dictionary<string, PageBase> _pages = new Dictionary<string, PageBase>();
        private PageBase _current;
        private readonly List<PageCatalog.Entry> _entries = PageCatalog.All();

        /// <summary>True when the user pressed Logout (Program then shows the login form again).</summary>
        public bool LoggedOut { get; private set; }

        public MainForm()
        {
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Wholesale Admin - Back-Office";
            Size = new Size(1320, 800);
            MinimumSize = new Size(1150, 700);
            StartPosition = FormStartPosition.CenterScreen;
            Font = Theme.BaseFont;
            BackColor = Theme.Background;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Margin = Padding.Empty;
            root.Padding = Padding.Empty;

            // sidebar (spans both rows)
            var side = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Navy,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, 0),
                Margin = Padding.Empty
            };
            var brand = new Label
            {
                Text = "WHOLESALE\nADMIN",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Width = 230, Height = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = Padding.Empty
            };
            side.Controls.Add(brand);

            foreach (PageCatalog.Entry entry in _entries)
            {
                PageCatalog.Entry captured = entry;
                var b = new Button
                {
                    Text = "   " + entry.Text,
                    Width = 230, Height = 48,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Theme.Navy,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 10.5f),
                    Margin = Padding.Empty,
                    Cursor = Cursors.Hand,
                    Tag = captured
                };
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = Theme.NavyLight;
                b.Click += (s, e) => ShowPage(captured);
                _navButtons.Add(b);
                side.Controls.Add(b);
            }
            var logout = new Button
            {
                Text = "   Logout",
                Width = 230, Height = 48,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(252, 165, 165),
                BackColor = Theme.Navy,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10.5f),
                Margin = new Padding(0, 24, 0, 0),
                Cursor = Cursors.Hand
            };
            logout.FlatAppearance.BorderSize = 0;
            logout.FlatAppearance.MouseOverBackColor = Theme.NavyLight;
            logout.Click += (s, e) =>
            {
                if (!Theme.Confirm(this, "Log out of the back-office application?")) return;
                LoggedOut = true;
                Close();
            };
            side.Controls.Add(logout);

            // header
            var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = Padding.Empty };
            _lblTitle.Font = Theme.TitleFont;
            _lblTitle.ForeColor = Theme.Navy;
            _lblTitle.AutoSize = true;
            _lblTitle.Left = 20;
            _lblTitle.Top = 16;
            _lblUser.ForeColor = Theme.TextMuted;
            _lblUser.AutoSize = true;
            _lblUser.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AuthUser u = Session.CurrentUser;
            _lblUser.Text = u == null ? "" : "Signed in as " + u.UserName + " (" + u.Role + ")";
            header.Controls.Add(_lblTitle);
            header.Controls.Add(_lblUser);
            header.Resize += (s, e) => _lblUser.Left = header.Width - _lblUser.Width - 20;
            _lblUser.Top = 22;

            _host.Dock = DockStyle.Fill;
            _host.Margin = Padding.Empty;
            _host.Padding = new Padding(10);

            root.Controls.Add(side, 0, 0);
            root.SetRowSpan(side, 2);
            root.Controls.Add(header, 1, 0);
            root.Controls.Add(_host, 1, 1);
            Controls.Add(root);

            Shown += (s, e) =>
            {
                if (_entries.Count > 0) ShowPage(_entries[0]);
                _lblUser.Left = header.Width - _lblUser.Width - 20;
            };
        }

        private void ShowPage(PageCatalog.Entry entry)
        {
            if (_current != null) _current.OnHidden();
            PageBase page;
            if (!_pages.TryGetValue(entry.Text, out page))
            {
                page = entry.Create();
                _pages[entry.Text] = page;
                _host.Controls.Add(page);
            }
            foreach (PageBase p in _pages.Values) p.Visible = ReferenceEquals(p, page);
            page.BringToFront();
            _current = page;
            _lblTitle.Text = page.PageTitle;
            foreach (Button b in _navButtons)
            {
                bool active = ReferenceEquals(b.Tag, entry);
                b.BackColor = active ? Theme.Primary : Theme.Navy;
                b.FlatAppearance.MouseOverBackColor = active ? Theme.Primary : Theme.NavyLight;
            }
            try { page.OnShown(); }
            catch (Exception ex) { Theme.Error(this, Db.Friendly(ex)); }
        }
    }
}
