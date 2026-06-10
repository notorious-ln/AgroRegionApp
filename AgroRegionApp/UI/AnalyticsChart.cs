using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace AgroRegionApp.UI
{
    internal enum AnalyticsChartKind
    {
        Bar,
        Line,
        GroupedBar
    }

    internal sealed class AnalyticsChartPoint
    {
        public string Label { get; set; }
        public decimal Value1 { get; set; }
        public decimal Value2 { get; set; }
        public string Series1Name { get; set; } = "Значение";
        public string Series2Name { get; set; } = "Значение 2";
    }

    internal sealed class AnalyticsChartControl : Control
    {
        private readonly ToolTip _toolTip = new ToolTip { AutoPopDelay = 4000, InitialDelay = 200, ReshowDelay = 100 };
        private readonly List<HitRegion> _hitRegions = new List<HitRegion>();
        private int _hoverIndex = -1;

        public AnalyticsChartKind ChartKind { get; set; } = AnalyticsChartKind.Bar;
        public IList<AnalyticsChartPoint> Points { get; set; } = new List<AnalyticsChartPoint>();
        public Color Series1Color { get; set; } = AppTheme.Blue;
        public Color Series2Color { get; set; } = Color.FromArgb(124, 58, 237);
        public bool UseThousandsSuffix { get; set; } = true;
        public bool IsCurrency { get; set; } = true;

        public AnalyticsChartControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Color.White;
            Height = 180;
            Cursor = Cursors.Hand;
            MouseMove += OnChartMouseMove;
            MouseLeave += (s, e) => { _hoverIndex = -1; _toolTip.RemoveAll(); Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            try
            {
                PaintChart(e.Graphics);
            }
            catch
            {
                e.Graphics.Clear(BackColor);
                DrawEmpty(e.Graphics);
            }
        }

        private void PaintChart(Graphics g)
        {
            _hitRegions.Clear();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);

            if (Points == null || Points.Count == 0)
            {
                DrawEmpty(g);
                return;
            }

            var plot = GetPlotRect();
            DrawGrid(g, plot);

            switch (ChartKind)
            {
                case AnalyticsChartKind.Bar:
                    DrawBarChart(g, plot, false);
                    break;
                case AnalyticsChartKind.GroupedBar:
                    DrawBarChart(g, plot, true);
                    DrawLegend(g, new[] { ("Склад №1", Series1Color), ("Склад №2", Series2Color) });
                    break;
                case AnalyticsChartKind.Line:
                    DrawLineChart(g, plot);
                    DrawLegend(g, new[] { ("Продажи", Series1Color), ("Закупки", Series2Color) });
                    break;
            }
        }

        private Rectangle GetPlotRect() =>
            new Rectangle(48, 8, Math.Max(Width - 64, 40), Math.Max(Height - 44, 40));

        private void DrawEmpty(Graphics g)
        {
            var text = "Нет данных для отображения";
            var size = g.MeasureString(text, AppTheme.FontUi);
            g.DrawString(text, AppTheme.FontUi, new SolidBrush(AppTheme.TextMuted),
                (Width - size.Width) / 2f, (Height - size.Height) / 2f);
        }

        private void DrawGrid(Graphics g, Rectangle plot)
        {
            using (var axisPen = new Pen(AppTheme.BorderLight))
            using (var gridPen = new Pen(Color.FromArgb(229, 231, 235)) { DashStyle = DashStyle.Dot })
            using (var labelBrush = new SolidBrush(AppTheme.TextMuted))
            {
                g.DrawRectangle(axisPen, plot);

                var maxVal = GetScaleMax();
                var steps = 4;
                for (var i = 0; i <= steps; i++)
                {
                    var y = plot.Bottom - (int)(plot.Height * (i / (float)steps));
                    if (i > 0)
                        g.DrawLine(gridPen, plot.Left, y, plot.Right, y);

                    var val = maxVal * i / steps;
                    var label = FormatAxis(val);
                    g.DrawString(label, AppTheme.FontUi, labelBrush, 2, y - 8);
                }

                var labelWidth = plot.Width / Math.Max(Points.Count, 1);
                for (var i = 0; i < Points.Count; i++)
                {
                    var x = plot.Left + labelWidth * i + labelWidth / 2f;
                    var label = Points[i].Label;
                    var size = g.MeasureString(label, AppTheme.FontUi);
                    g.DrawString(label, AppTheme.FontUi, labelBrush, x - size.Width / 2f, plot.Bottom + 4);
                }
            }
        }

        private decimal GetMaxValue()
        {
            if (Points == null || Points.Count == 0) return 0;
            if (ChartKind == AnalyticsChartKind.GroupedBar)
                return Points.Max(p => Math.Max(p.Value1, p.Value2));
            if (ChartKind == AnalyticsChartKind.Line)
                return Points.Max(p => Math.Max(p.Value1, p.Value2));
            return Points.Max(p => p.Value1);
        }

        private decimal GetScaleMax()
        {
            var max = GetMaxValue();
            return max <= 0 ? 1 : max * 1.1m;
        }

        private string FormatAxis(decimal value)
        {
            if (UseThousandsSuffix && value >= 1000)
                return $"{value / 1000m:0}k";
            return value >= 100 ? $"{value:0}" : $"{value:0.#}";
        }

        private void DrawBarChart(Graphics g, Rectangle plot, bool grouped)
        {
            var maxVal = GetScaleMax();
            if (Points.Count == 0) return;
            var slotWidth = plot.Width / (float)Points.Count;
            var barWidth = grouped ? slotWidth * 0.28f : slotWidth * 0.55f;

            for (var i = 0; i < Points.Count; i++)
            {
                var point = Points[i];
                var slotX = plot.Left + slotWidth * i;

                if (grouped)
                {
                    DrawSingleBar(g, plot, maxVal, slotX + slotWidth * 0.18f, barWidth, point.Value1, Series1Color);
                    DrawSingleBar(g, plot, maxVal, slotX + slotWidth * 0.54f, barWidth, point.Value2, Series2Color);
                }
                else
                {
                    DrawSingleBar(g, plot, maxVal, slotX + (slotWidth - barWidth) / 2f, barWidth, point.Value1, Series1Color);
                }

                var hitRect = new RectangleF(slotX, plot.Top, slotWidth, plot.Height);
                _hitRegions.Add(new HitRegion { Index = i, Bounds = hitRect });
            }

            if (_hoverIndex >= 0 && _hoverIndex < Points.Count)
            {
                var slotX = plot.Left + slotWidth * _hoverIndex;
                using (var brush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                    g.FillRectangle(brush, slotX, plot.Top, slotWidth, plot.Height);
            }
        }

        private static void DrawSingleBar(Graphics g, Rectangle plot, decimal maxVal, float x, float width, decimal value, Color color)
        {
            if (value <= 0) return;
            var height = (float)(plot.Height * (double)(value / maxVal));
            var rect = new RectangleF(x, plot.Bottom - height, width, height);
            using (var brush = new SolidBrush(color))
                g.FillRectangle(brush, rect);
        }

        private void DrawLineChart(Graphics g, Rectangle plot)
        {
            var maxVal = GetScaleMax();
            if (Points.Count == 0) return;

            var slotWidth = Points.Count > 1
                ? plot.Width / (float)(Points.Count - 1)
                : plot.Width / 2f;
            var pts1 = new List<PointF>();
            var pts2 = new List<PointF>();

            for (var i = 0; i < Points.Count; i++)
            {
                var x = plot.Left + slotWidth * i;
                pts1.Add(new PointF(x, plot.Bottom - (float)(plot.Height * (double)(Points[i].Value1 / maxVal))));
                pts2.Add(new PointF(x, plot.Bottom - (float)(plot.Height * (double)(Points[i].Value2 / maxVal))));

                var hitRect = new RectangleF(x - slotWidth / 2f, plot.Top, slotWidth, plot.Height);
                _hitRegions.Add(new HitRegion { Index = i, Bounds = hitRect });
            }

            DrawSmoothLine(g, pts1, Series1Color);
            DrawSmoothLine(g, pts2, Series2Color);

            for (var i = 0; i < Points.Count; i++)
            {
                var x = plot.Left + slotWidth * i;
                FillDot(g, x, pts1[i].Y, Series1Color);
                FillDot(g, x, pts2[i].Y, Series2Color);
            }

            if (_hoverIndex >= 0 && _hoverIndex < Points.Count)
            {
                var slotX = plot.Left + slotWidth * _hoverIndex - slotWidth / 2f;
                using (var brush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                    g.FillRectangle(brush, slotX, plot.Top, slotWidth, plot.Height);
            }
        }

        private static void DrawSmoothLine(Graphics g, IList<PointF> points, Color color)
        {
            if (points.Count < 2) return;

            var allSame = true;
            for (var i = 1; i < points.Count; i++)
            {
                if (Math.Abs(points[i].Y - points[0].Y) > 0.5f)
                {
                    allSame = false;
                    break;
                }
            }

            using (var pen = new Pen(color, 2f) { LineJoin = LineJoin.Round })
            {
                if (points.Count == 2 || allSame)
                    g.DrawLines(pen, points.ToArray());
                else
                    g.DrawCurve(pen, points.ToArray(), 0.35f);
            }
        }

        private static void FillDot(Graphics g, float x, float y, Color color)
        {
            const float r = 4f;
            using (var brush = new SolidBrush(color))
                g.FillEllipse(brush, x - r, y - r, r * 2, r * 2);
            using (var pen = new Pen(Color.White, 1.5f))
                g.DrawEllipse(pen, x - r, y - r, r * 2, r * 2);
        }

        private void DrawLegend(Graphics g, (string Label, Color Color)[] items)
        {
            var x = Width / 2f - items.Length * 55f;
            var y = Height - 18f;
            foreach (var item in items)
            {
                using (var brush = new SolidBrush(item.Color))
                    g.FillEllipse(brush, x, y + 3, 8, 8);
                g.DrawString(item.Label, AppTheme.FontUi, new SolidBrush(AppTheme.TextBody), x + 12, y);
                x += 110;
            }
        }

        private void OnChartMouseMove(object sender, MouseEventArgs e)
        {
            var index = -1;
            foreach (var region in _hitRegions)
            {
                if (region.Bounds.Contains(e.Location))
                {
                    index = region.Index;
                    break;
                }
            }

            if (index == _hoverIndex) return;
            _hoverIndex = index;
            Invalidate();

            _toolTip.RemoveAll();
            if (index < 0 || index >= Points.Count) return;

            var p = Points[index];
            string text;
            if (ChartKind == AnalyticsChartKind.GroupedBar)
            {
                text = $"{p.Label}\r\n{p.Series1Name}: {FormatValue(p.Value1)}\r\n{p.Series2Name}: {FormatValue(p.Value2)}";
            }
            else if (ChartKind == AnalyticsChartKind.Line)
            {
                text = $"{p.Label}\r\n{p.Series1Name}: {FormatValue(p.Value1)}\r\n{p.Series2Name}: {FormatValue(p.Value2)}";
            }
            else
            {
                text = $"{p.Label}\r\n{p.Series1Name}: {FormatValue(p.Value1)}";
            }

            _toolTip.Show(text, this, e.Location.X + 12, e.Location.Y - 8, 3000);
        }

        private string FormatValue(decimal value) =>
            IsCurrency ? AnalyticsFormat.Money(value) : $"{value:0}";

        private sealed class HitRegion
        {
            public int Index { get; set; }
            public RectangleF Bounds { get; set; }
        }
    }

    internal static class AnalyticsFormat
    {
        public static string Money(decimal value) =>
            value.ToString("N0").Replace('\u00A0', ' ') + " ₽";

        public static string Diff(decimal value)
        {
            var formatted = value.ToString("N0").Replace('\u00A0', ' ');
            return value >= 0 ? $"+{formatted}" : formatted;
        }
    }
}
