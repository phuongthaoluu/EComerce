using System;
using System.Drawing;
using System.Windows.Forms;

namespace Wholesale.WinForms.UI
{
    /// <summary>Colours, fonts and small control factories so every screen looks the same.</summary>
    public static class Theme
    {
        public static readonly Color Primary = Color.FromArgb(37, 99, 235);
        public static readonly Color PrimaryDark = Color.FromArgb(29, 78, 216);
        public static readonly Color Navy = Color.FromArgb(17, 24, 39);
        public static readonly Color NavyLight = Color.FromArgb(31, 41, 55);
        public static readonly Color Background = Color.FromArgb(243, 244, 246);
        public static readonly Color Card = Color.White;
        public static readonly Color Border = Color.FromArgb(229, 231, 235);
        public static readonly Color TextMuted = Color.FromArgb(107, 114, 128);
        public static readonly Color Danger = Color.FromArgb(220, 38, 38);
        public static readonly Color Success = Color.FromArgb(22, 163, 74);
        public static readonly Color Warning = Color.FromArgb(217, 119, 6);
        public static readonly Color Grey = Color.FromArgb(75, 85, 99);

        public static readonly Font BaseFont = new Font("Segoe UI", 9.75f);
        public static readonly Font BoldFont = new Font("Segoe UI", 9.75f, FontStyle.Bold);
        public static readonly Font TitleFont = new Font("Segoe UI", 15f, FontStyle.Bold);
        public static readonly Font SectionFont = new Font("Segoe UI", 11f, FontStyle.Bold);
        public static readonly Font KpiFont = new Font("Segoe UI", 18f, FontStyle.Bold);

        public static Button MakeButton(string text, Color back, int width)
        {
            var b = new Button
            {
                Text = text,
                Width = width,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = Color.White,
                Font = BoldFont,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 8),
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        public static Label MakeLabel(string text, bool bold)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = bold ? BoldFont : BaseFont,
                ForeColor = bold ? Navy : TextMuted,
                Margin = new Padding(0, 6, 0, 2)
            };
        }

        public static Panel MakeCard()
        {
            return new Panel { BackColor = Card, Dock = DockStyle.Fill, Padding = new Padding(12), Margin = new Padding(6) };
        }

        public static void StyleGrid(DataGridView g)
        {
            g.Dock = DockStyle.Fill;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.RowHeadersVisible = false;
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Border;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersDefaultCellStyle.BackColor = Navy;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = BoldFont;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Navy;
            g.ColumnHeadersHeight = 36;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.Font = BaseFont;
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            g.DefaultCellStyle.SelectionForeColor = Navy;
            g.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            g.RowTemplate.Height = 30;
        }

        public static string Money(object v)
        {
            decimal d = (v == null || v == DBNull.Value) ? 0m : Convert.ToDecimal(v);
            return d.ToString("N0") + " đ";
        }

        public static void Info(IWin32Window owner, string msg)
        {
            MessageBox.Show(owner, msg, "Wholesale Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Warn(IWin32Window owner, string msg)
        {
            MessageBox.Show(owner, msg, "Wholesale Admin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Error(IWin32Window owner, string msg)
        {
            MessageBox.Show(owner, msg, "Wholesale Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool Confirm(IWin32Window owner, string msg)
        {
            return MessageBox.Show(owner, msg, "Please confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}
