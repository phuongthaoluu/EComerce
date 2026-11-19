using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Wholesale.WinForms.UI
{
    /// <summary>Small dependency-free horizontal bar chart (the old Chart control does not exist in modern .NET).</summary>
    public class BarChartControl : Control
    {
        private readonly List<string> _labels = new List<string>();
        private readonly List<double> _values = new List<double>();

        public BarChartControl()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
            Font = Theme.BaseFont;
        }

        public void SetData(IList<string> labels, IList<double> values)
        {
            _labels.Clear();
            _values.Clear();
            for (int i = 0; i < labels.Count && i < values.Count; i++)
            {
                _labels.Add(labels[i]);
                _values.Add(values[i]);
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            int n = _values.Count;
            if (n == 0)
            {
                using (var f = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString("No data for the selected filter", Font, Brushes.Gray, ClientRectangle, f);
                return;
            }

            double max = 0;
            foreach (double v in _values) if (v > max) max = v;
            if (max <= 0) max = 1;

            float labelW = Math.Min(190f, Width * 0.38f);
            float valueW = 62f;
            float plotLeft = labelW + 8f;
            float plotW = Math.Max(10f, Width - plotLeft - valueW - 8f);
            float top = 6f;
            float rowH = (Height - top - 6f) / n;
            float barH = Math.Min(24f, rowH * 0.62f);

            using (var labelFmt = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            using (var valueFmt = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
            using (var barBrush = new SolidBrush(Theme.Primary))
            using (var textBrush = new SolidBrush(Theme.Navy))
            using (var axisPen = new Pen(Theme.Border))
            {
                g.DrawLine(axisPen, plotLeft, top, plotLeft, Height - 6f);
                for (int i = 0; i < n; i++)
                {
                    float y = top + i * rowH;
                    g.DrawString(_labels[i], Font, textBrush, new RectangleF(2f, y, labelW, rowH), labelFmt);
                    float w = (float)(_values[i] / max) * plotW;
                    float barY = y + (rowH - barH) / 2f;
                    g.FillRectangle(barBrush, plotLeft, barY, Math.Max(2f, w), barH);
                    g.DrawString(_values[i].ToString("N1"), Font, textBrush, new RectangleF(plotLeft + Math.Max(2f, w) + 4f, y, valueW + 40f, rowH), valueFmt);
                }
            }
        }
    }
}
