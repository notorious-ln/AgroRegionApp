using System.Drawing;
using System.Windows.Forms;

namespace AgroRegionApp.UI
{
    internal class ModalForm : Form
    {
        public Panel BodyPanel { get; }

        public ModalForm(string title, int width, int height)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(width, height);
            Font = AppTheme.FontUi;
            BackColor = Color.FromArgb(245, 246, 248);
            Padding = new Padding(1);

            var border = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(156, 163, 175) };
            var inner = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = Color.FromArgb(156, 163, 175) };
            var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 246, 248) };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 26,
                BackColor = AppTheme.Navy
            };
            header.Controls.Add(new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = AppTheme.FontUiBold,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            });

            var btnClose = new Button
            {
                Text = "✕",
                Dock = DockStyle.Right,
                Width = 32,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = AppTheme.Navy,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            header.Controls.Add(btnClose);

            BodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(245, 246, 248)
            };

            content.Controls.Add(BodyPanel);
            content.Controls.Add(header);
            inner.Controls.Add(content);
            border.Controls.Add(inner);
            Controls.Add(border);
        }
    }
}
