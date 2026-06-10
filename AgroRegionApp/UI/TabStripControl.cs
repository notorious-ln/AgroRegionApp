using System;
using System.Drawing;
using System.Windows.Forms;

namespace AgroRegionApp.UI
{
    internal sealed class TabStripControl : Panel
    {
        private readonly Button[] _buttons;

        public event Action<int> TabChanged;

        public TabStripControl(string[] labels)
        {
            Height = 30;
            Dock = DockStyle.Top;
            BackColor = AppTheme.ContentBg;
            Margin = new Padding(0, 0, 0, 8);

            _buttons = new Button[labels.Length];
            for (var i = 0; i < labels.Length; i++)
            {
                var index = i;
                var btn = new Button
                {
                    Text = labels[i],
                    Height = 28,
                    Width = TextRenderer.MeasureText(labels[i], AppTheme.FontUi).Width + 24,
                    Location = new Point(i == 0 ? 0 : _buttons[i - 1].Right, 0),
                    FlatStyle = FlatStyle.Flat,
                    Font = AppTheme.FontUi,
                    Cursor = Cursors.Hand,
                    Tag = index
                };
                btn.FlatAppearance.BorderColor = AppTheme.Border;
                btn.FlatAppearance.BorderSize = 1;
                btn.Click += (s, e) => SelectTab(index);
                _buttons[i] = btn;
                Controls.Add(btn);
            }

            SelectTab(0, notify: false);
        }

        public void SelectTab(int index, bool notify = true)
        {
            for (var j = 0; j < _buttons.Length; j++)
                StyleTab(_buttons[j], j == index);
            if (notify)
                TabChanged?.Invoke(index);
        }

        private static void StyleTab(Button btn, bool active)
        {
            btn.BackColor = active ? Color.White : AppTheme.TabInactive;
            btn.ForeColor = active ? AppTheme.TextPrimary : AppTheme.TextMuted;
            btn.Font = active ? AppTheme.FontUiBold : AppTheme.FontUi;
        }
    }
}
