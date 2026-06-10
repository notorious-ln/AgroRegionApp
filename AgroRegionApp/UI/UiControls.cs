using System;
using System.Drawing;
using System.Windows.Forms;

namespace AgroRegionApp.UI
{
    internal static class UiControls
    {
        public static Button CreateButton(string text, bool primary = false, int width = 0, bool danger = false)
        {
            var btn = new Button
            {
                Text = text,
                Height = 26,
                Width = width > 0 ? width : TextRenderer.MeasureText(text, AppTheme.FontUi).Width + 28,
                FlatStyle = FlatStyle.Flat,
                Font = AppTheme.FontUi,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 6, 0)
            };

            if (primary)
            {
                btn.BackColor = AppTheme.Blue;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderColor = Color.FromArgb(26, 90, 154);
            }
            else if (danger)
            {
                btn.BackColor = Color.White;
                btn.ForeColor = AppTheme.Danger;
                btn.FlatAppearance.BorderColor = AppTheme.Border;
            }
            else
            {
                btn.BackColor = Color.White;
                btn.ForeColor = AppTheme.TextBody;
                btn.FlatAppearance.BorderColor = AppTheme.Border;
            }

            btn.FlatAppearance.BorderSize = 1;
            return btn;
        }

        public static GroupBox CreateGroupBox(string title)
        {
            return new GroupBox
            {
                Text = title.ToUpperInvariant(),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = AppTheme.TextMuted,
                Padding = new Padding(10, 16, 10, 10),
                BackColor = AppTheme.CardBg
            };
        }

        public static DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(240, 240, 240),
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 26,
                RowTemplate = { Height = 24 },
                Font = AppTheme.FontUi
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.GridHeader;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextBody;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.25f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = AppTheme.GridHeader;
            grid.DefaultCellStyle.SelectionBackColor = AppTheme.GridSelect;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = AppTheme.GridAlt;
            grid.DataError += (s, e) => e.ThrowException = false;
            return grid;
        }

        public static void BindGrid(DataGridView grid, object data)
        {
            grid.DataSource = null;
            grid.DataSource = data;
        }

        public static Panel CreateInfoBar(string text, Color bg, Color border, Color? foreColor = null)
        {
            var panel = new Panel
            {
                Height = 36,
                Dock = DockStyle.Top,
                BackColor = bg,
                Padding = new Padding(10, 8, 10, 8),
                Margin = new Padding(0, 0, 0, 8)
            };
            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(border))
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };
            panel.Controls.Add(new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = AppTheme.FontUi,
                ForeColor = foreColor ?? AppTheme.TextBody,
                BackColor = Color.Transparent
            });
            return panel;
        }

        public static TabStripControl CreateTabStrip(string[] labels)
        {
            return new TabStripControl(labels);
        }

        public static Label CreateFieldLabel(string text, int width = 110)
        {
            return new Label
            {
                Text = text,
                Width = width,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.TextBody
            };
        }

        public static TextBox CreateFieldBox(bool readOnly = false)
        {
            return new TextBox
            {
                Height = 24,
                Font = AppTheme.FontUi,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = readOnly,
                BackColor = readOnly ? Color.FromArgb(243, 244, 246) : Color.White
            };
        }

        public static Panel CreateKpiCard(string icon, string label, string value, Color valueColor)
        {
            var card = new Panel
            {
                Height = 72,
                Margin = new Padding(0, 0, 8, 0),
                BackColor = Color.White,
                Padding = new Padding(12, 10, 12, 10)
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(AppTheme.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            card.Controls.Add(new Label
            {
                Text = icon,
                Location = new Point(10, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 14f)
            });
            card.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(44, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = AppTheme.TextMuted
            });
            card.Controls.Add(new Label
            {
                Text = value,
                Location = new Point(44, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = valueColor
            });
            return card;
        }
    }
}
