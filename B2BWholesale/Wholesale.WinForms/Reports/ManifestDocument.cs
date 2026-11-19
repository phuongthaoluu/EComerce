using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using Wholesale.WinForms.Data;

namespace Wholesale.WinForms.Reports
{
    /// <summary>Everything the Shipping Manifest needs for one order.</summary>
    public class ManifestData
    {
        public int OrderId;
        public DateTime OrderDate;
        public string Status;
        public string AgentName;
        public string Address;
        public string Phone;
        public DataTable Lines;
        public int TotalQty;
        public decimal TotalAmount;

        public static ManifestData Load(int orderId)
        {
            DataRow h = OrderRepository.GetHeader(orderId);
            if (h == null) throw new InvalidOperationException("Order #" + orderId + " was not found.");
            DataTable lines = OrderRepository.GetLines(orderId);
            var d = new ManifestData
            {
                OrderId = orderId,
                OrderDate = Convert.ToDateTime(h["OrderDate"]),
                Status = Convert.ToString(h["OrderStatus"]),
                AgentName = Convert.ToString(h["AgentName"]),
                Address = Convert.ToString(h["Address"]),
                Phone = Convert.ToString(h["Phone"]),
                Lines = lines
            };
            foreach (DataRow r in lines.Rows)
            {
                d.TotalQty += Convert.ToInt32(r["Quantity"]);
                d.TotalAmount += Convert.ToDecimal(r["LineTotal"]);
            }
            return d;
        }
    }

    /// <summary>
    /// Task 1.3 - Shipping Manifest. A PrintDocument, so the same code gives print preview, printing
    /// and PDF export (through the "Microsoft Print to PDF" printer that ships with Windows 10/11).
    /// </summary>
    public class ManifestDocument : PrintDocument
    {
        private static readonly CultureInfo Vn = new CultureInfo("vi-VN");
        private static readonly Font FTitle = new Font("Arial", 20f, FontStyle.Bold);
        private static readonly Font FCompany = new Font("Arial", 12f, FontStyle.Bold);
        private static readonly Font FBold = new Font("Arial", 10f, FontStyle.Bold);
        private static readonly Font FBody = new Font("Arial", 10f);
        private static readonly Font FSmall = new Font("Arial", 8.5f);

        private readonly ManifestData _d;
        private int _row;
        private int _page;

        public ManifestDocument(ManifestData data)
        {
            _d = data;
            DocumentName = "Shipping Manifest - Order " + data.OrderId;
            DefaultPageSettings.Margins = new Margins(60, 60, 60, 60);
            PrintController = new StandardPrintController();   // no "Printing page..." popup
        }

        protected override void OnBeginPrint(PrintEventArgs e)
        {
            base.OnBeginPrint(e);
            _row = 0;
            _page = 0;
        }

        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            base.OnPrintPage(e);
            Graphics g = e.Graphics;
            Rectangle m = e.MarginBounds;
            _page++;
            float y = m.Top;

            var right = new StringFormat { Alignment = StringAlignment.Far };
            var center = new StringFormat { Alignment = StringAlignment.Center };
            var cell = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
            var cellRight = new StringFormat { Alignment = StringAlignment.Far, FormatFlags = StringFormatFlags.NoWrap };

            // ---- letterhead
            g.DrawString(Db.GetSetting("CompanyName", "Wholesale Co."), FCompany, Brushes.Black, m.Left, y);
            g.DrawString(Db.GetSetting("CompanyAddress", "") + "  |  Tel: " + Db.GetSetting("CompanyPhone", ""), FSmall, Brushes.DimGray, m.Left, y + 22);
            g.DrawString("SHIPPING MANIFEST", FTitle, Brushes.Black, new RectangleF(m.Left, y, m.Width, 36), right);
            y += 52;
            g.DrawLine(new Pen(Color.Black, 2f), m.Left, y, m.Right, y);
            y += 14;

            // ---- order + consignee block
            float half = m.Width / 2f;
            g.DrawString("Order No:", FBold, Brushes.Black, m.Left, y);
            g.DrawString("#" + _d.OrderId, FBody, Brushes.Black, m.Left + 90, y);
            g.DrawString("Order date:", FBold, Brushes.Black, m.Left, y + 22);
            g.DrawString(_d.OrderDate.ToString("dd/MM/yyyy HH:mm", Vn), FBody, Brushes.Black, m.Left + 90, y + 22);
            g.DrawString("Status:", FBold, Brushes.Black, m.Left, y + 44);
            g.DrawString(_d.Status, FBody, Brushes.Black, m.Left + 90, y + 44);

            g.DrawString("SHIP TO", FBold, Brushes.Black, m.Left + half, y);
            g.DrawString(_d.AgentName, FBody, Brushes.Black, new RectangleF(m.Left + half, y + 22, half, 20));
            g.DrawString(_d.Address, FBody, Brushes.Black, new RectangleF(m.Left + half, y + 42, half, 44));
            g.DrawString("Tel: " + _d.Phone, FBody, Brushes.Black, m.Left + half, y + 88);
            y += 118;

            // ---- table header
            float[] frac = { 0.06f, 0.36f, 0.20f, 0.08f, 0.15f, 0.15f };
            string[] head = { "No", "Item", "Size / packing", "Qty", "Unit price", "Amount" };
            var x = new float[frac.Length + 1];
            x[0] = m.Left;
            for (int i = 0; i < frac.Length; i++) x[i + 1] = x[i] + m.Width * frac[i];
            const float rowH = 26f;

            g.FillRectangle(Brushes.Gainsboro, m.Left, y, m.Width, rowH);
            g.DrawRectangle(Pens.Black, m.Left, y, m.Width, rowH);
            for (int i = 0; i < head.Length; i++)
            {
                StringFormat f = i >= 3 ? cellRight : cell;
                g.DrawString(head[i], FBold, Brushes.Black, new RectangleF(x[i] + 4, y + 5, x[i + 1] - x[i] - 8, rowH), f);
            }
            y += rowH;

            // ---- rows
            bool more = false;
            while (_row < _d.Lines.Rows.Count)
            {
                if (y + rowH > m.Bottom - 24) { more = true; break; }
                DataRow r = _d.Lines.Rows[_row];
                g.DrawRectangle(Pens.Gray, m.Left, y, m.Width, rowH);
                string[] vals =
                {
                    (_row + 1).ToString(),
                    Convert.ToString(r["ItemName"]),
                    Convert.ToString(r["Size"]),
                    Convert.ToInt32(r["Quantity"]).ToString("N0", Vn),
                    Convert.ToDecimal(r["UnitAmount"]).ToString("N0", Vn),
                    Convert.ToDecimal(r["LineTotal"]).ToString("N0", Vn)
                };
                for (int i = 0; i < vals.Length; i++)
                {
                    StringFormat f = i >= 3 ? cellRight : cell;
                    g.DrawString(vals[i], FBody, Brushes.Black, new RectangleF(x[i] + 4, y + 5, x[i + 1] - x[i] - 8, rowH), f);
                }
                y += rowH;
                _row++;
            }

            // ---- totals + signatures only after the last line
            if (!more)
            {
                if (y + 170 > m.Bottom) more = true;       // not enough room: finish on the next page
                else
                {
                    y += 8;
                    g.DrawString("Total quantity: " + _d.TotalQty.ToString("N0", Vn), FBold, Brushes.Black, new RectangleF(m.Left, y, m.Width, 22), right);
                    y += 24;
                    g.DrawString("TOTAL AMOUNT (VND): " + _d.TotalAmount.ToString("N0", Vn), FCompany, Brushes.Black, new RectangleF(m.Left, y, m.Width, 26), right);
                    y += 60;
                    string[] sig = { "Prepared by (Warehouse)", "Delivered by (Driver)", "Received by (Agent)" };
                    float w = m.Width / 3f;
                    for (int i = 0; i < 3; i++)
                    {
                        g.DrawString(sig[i], FBold, Brushes.Black, new RectangleF(m.Left + w * i, y, w, 20), center);
                        g.DrawString("(Signature, full name)", FSmall, Brushes.DimGray, new RectangleF(m.Left + w * i, y + 18, w, 18), center);
                        g.DrawLine(Pens.Black, m.Left + w * i + 25, y + 78, m.Left + w * (i + 1) - 25, y + 78);
                    }
                }
            }

            g.DrawString("Page " + _page, FSmall, Brushes.DimGray, new RectangleF(m.Left, m.Bottom - 8, m.Width, 16), center);
            e.HasMorePages = more;
        }
    }
}
