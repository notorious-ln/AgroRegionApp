using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AgroRegionApp.Models;
using AgroRegionApp.Navigation;
using AgroRegionApp.UI;
using AgroRegionApp.Views;

namespace AgroRegionApp.Forms
{
    public partial class MainForm : Form
    {
        private readonly AuthenticatedUser _user;
        private readonly IReadOnlyList<NavItem> _navItems;
        private readonly Dictionary<NavSection, Control> _screens = new Dictionary<NavSection, Control>();
        private Panel _contentHost;
        private Panel _contentWrapper;
        private Label _lblSectionTitle;
        private Label _lblSectionIcon;
        private Label _lblDateTime;
        private Label _lblTitleSection;
        private readonly List<Panel> _navButtons = new List<Panel>();

        private NavSection _currentSection;

        public bool ReturnToLogin { get; private set; }

        public MainForm(AuthenticatedUser user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _navItems = RoleNavigation.GetItemsForRole(user.RoleName);

            if (_navItems.Count == 0)
            {
                MessageBox.Show(
                    "Для вашей роли не настроены доступные разделы.",
                    AppBranding.SystemTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            InitializeComponent();
            BuildShell();

            _contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.ContentBg,
                Padding = new Padding(12)
            };

            _contentWrapper.Controls.Add(_contentHost);

            if (_navItems.Count > 0)
                ShowSection(_navItems[0].Section);
        }

        private void BuildShell()
        {
            var titleBar = new Panel
            {
                Name = "titleBar",
                Dock = DockStyle.Top,
                Height = 28,
                BackColor = AppTheme.Navy
            };

            var icon = new Label
            {
                Text = "🌾",
                Location = new Point(10, 0),
                Size = new Size(22, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10f)
            };

            var title = new Label
            {
                Text = AppBranding.SystemTitle,
                Location = new Point(34, 0),
                AutoSize = true,
                Height = 28,
                Font = AppTheme.FontUiBold,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblTitleSection = new Label
            {
                Location = new Point(200, 0),
                AutoSize = true,
                Height = 28,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.SidebarMuted,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "—"
            };

            titleBar.Controls.Add(icon);
            titleBar.Controls.Add(title);
            titleBar.Controls.Add(_lblTitleSection);
            titleBar.Controls.Add(CreateTitleBarButtons(titleBar));

            var body = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.ContentBg };

            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 180,
                BackColor = AppTheme.Navy
            };

            var navPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Navy,
                Padding = new Padding(0, 8, 0, 0)
            };

            for (var i = _navItems.Count - 1; i >= 0; i--)
            {
                var btn = CreateNavButton(_navItems[i]);
                navPanel.Controls.Add(btn);
                _navButtons.Add(btn);
            }

            var userCard = CreateUserCard();
            userCard.Dock = DockStyle.Top;
            var logoutPanel = CreateLogoutPanel();

            sidebar.Controls.Add(navPanel);
            sidebar.Controls.Add(userCard);
            sidebar.Controls.Add(logoutPanel);

            var mainArea = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.ContentBg };

            var contentHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.White,
                Padding = new Padding(12, 0, 12, 0)
            };
            contentHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(AppTheme.BorderLight))
                    e.Graphics.DrawLine(pen, 0, contentHeader.Height - 1, contentHeader.Width, contentHeader.Height - 1);
            };

            _lblSectionIcon = new Label
            {
                Location = new Point(12, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 11f)
            };
            _lblSectionTitle = new Label
            {
                Location = new Point(40, 9),
                AutoSize = true,
                Font = AppTheme.FontTitle,
                ForeColor = AppTheme.TextPrimary
            };
            _lblDateTime = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = AppTheme.TextLight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Text = DateTime.Now.ToString("g")
            };
            contentHeader.Controls.Add(_lblSectionIcon);
            contentHeader.Controls.Add(_lblSectionTitle);
            contentHeader.Controls.Add(_lblDateTime);
            contentHeader.Resize += (s, e) =>
                _lblDateTime.Location = new Point(contentHeader.Width - _lblDateTime.Width - 12, 10);

            _contentWrapper = new Panel
            {
                Name = "contentWrapper",
                Dock = DockStyle.Fill,
                BackColor = AppTheme.ContentBg
            };

            mainArea.Controls.Add(_contentWrapper);
            mainArea.Controls.Add(contentHeader);

            body.Controls.Add(mainArea);
            body.Controls.Add(sidebar);

            var statusBar = CreateStatusBar();
            statusBar.Dock = DockStyle.Bottom;

            Controls.Add(body);
            Controls.Add(statusBar);
            Controls.Add(titleBar);

            var timer = new Timer { Interval = 30000 };
            timer.Tick += (s, e) => _lblDateTime.Text = DateTime.Now.ToString("g");
            timer.Start();
        }

        private Panel CreateUserCard()
        {
            var card = new Panel
            {
                Height = 88,
                BackColor = AppTheme.Navy,
                Padding = new Padding(12, 12, 12, 8)
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(AppTheme.SidebarBorder))
                    e.Graphics.DrawLine(pen, 0, card.Height - 1, card.Width, card.Height - 1);
            };

            var avatar = new Panel
            {
                Location = new Point(12, 12),
                Size = new Size(40, 40),
                BackColor = AppTheme.Blue
            };
            avatar.Paint += (s, e) =>
            {
                var initial = string.IsNullOrEmpty(_user.DisplayName) ? "?" : _user.DisplayName.Substring(0, 1);
                TextRenderer.DrawText(e.Graphics, initial, new Font("Segoe UI", 12f, FontStyle.Bold),
                    avatar.ClientRectangle, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            var name = new Label
            {
                Text = _user.DisplayName,
                Location = new Point(12, 56),
                Size = new Size(156, 16),
                Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            var role = new Label
            {
                Text = _user.RoleName,
                Location = new Point(12, 72),
                Size = new Size(156, 14),
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = AppTheme.SidebarMuted,
                BackColor = Color.Transparent
            };

            card.Controls.Add(avatar);
            card.Controls.Add(name);
            card.Controls.Add(role);
            return card;
        }

        private Panel CreateNavButton(NavItem item)
        {
            var state = new NavButtonState { Item = item };
            var panel = new Panel
            {
                Height = 32,
                Dock = DockStyle.Top,
                Cursor = Cursors.Hand,
                Tag = state,
                BackColor = AppTheme.Navy
            };

            var icon = new Label
            {
                Text = item.Icon,
                Location = new Point(14, 6),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.Transparent
            };
            var label = new Label
            {
                Text = item.Label,
                Location = new Point(42, 8),
                AutoSize = true,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.SidebarText,
                BackColor = Color.Transparent
            };

            panel.Controls.Add(icon);
            panel.Controls.Add(label);
            panel.Click += (s, e) => ShowSection(item.Section);
            foreach (Control c in panel.Controls)
                c.Click += (s, e) => ShowSection(item.Section);

            ApplyNavButtonStyle(panel);
            AttachNavHover(panel);
            return panel;
        }

        private sealed class NavButtonState
        {
            public NavItem Item { get; set; }
            public bool Active { get; set; }
            public bool Hovered { get; set; }
        }

        private static void ApplyNavButtonStyle(Panel panel)
        {
            var state = (NavButtonState)panel.Tag;
            panel.BackColor = state.Active
                ? AppTheme.Blue
                : state.Hovered ? AppTheme.BlueHover : AppTheme.Navy;

            foreach (Control child in panel.Controls)
            {
                if (child is Label lbl)
                {
                    lbl.ForeColor = state.Active || state.Hovered
                        ? Color.White
                        : child.Left < 20 ? Color.White : AppTheme.SidebarText;
                    lbl.BackColor = Color.Transparent;
                    lbl.Font = state.Active ? AppTheme.FontUiBold : AppTheme.FontUi;
                }
            }
        }

        private static void AttachNavHover(Panel panel)
        {
            void SetHover(bool on)
            {
                var state = (NavButtonState)panel.Tag;
                state.Hovered = on;
                ApplyNavButtonStyle(panel);
            }

            void Wire(Control control)
            {
                control.MouseEnter += (s, e) => SetHover(true);
                control.MouseLeave += (s, e) =>
                {
                    if (!panel.ClientRectangle.Contains(panel.PointToClient(Cursor.Position)))
                        SetHover(false);
                };
            }

            Wire(panel);
            foreach (Control child in panel.Controls)
                Wire(child);
        }

        private Panel CreateLogoutPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = AppTheme.Navy,
                Cursor = Cursors.Hand
            };
            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(AppTheme.SidebarBorder))
                    e.Graphics.DrawLine(pen, 0, 0, panel.Width, 0);
            };

            var icon = new Label
            {
                Text = "🚪",
                Location = new Point(14, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            var label = new Label
            {
                Text = "Выйти из профиля",
                Location = new Point(42, 14),
                AutoSize = true,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.SidebarText,
                BackColor = Color.Transparent
            };

            EventHandler logout = (s, e) => ConfirmLogout();
            panel.Click += logout;
            icon.Click += logout;
            label.Click += logout;
            panel.Controls.Add(icon);
            panel.Controls.Add(label);
            AttachSidebarHover(panel, AppTheme.LogoutHover);
            return panel;
        }

        private static void AttachSidebarHover(Panel panel, Color hoverBg)
        {
            var normalBg = AppTheme.Navy;
            var normalText = AppTheme.SidebarText;

            void SetHover(bool on)
            {
                panel.BackColor = on ? hoverBg : normalBg;
                foreach (Control child in panel.Controls)
                {
                    if (child is Label lbl)
                    {
                        lbl.ForeColor = on ? Color.White : normalText;
                        lbl.BackColor = Color.Transparent;
                    }
                }
            }

            void Wire(Control control)
            {
                control.MouseEnter += (s, e) => SetHover(true);
                control.MouseLeave += (s, e) =>
                {
                    if (!panel.ClientRectangle.Contains(panel.PointToClient(Cursor.Position)))
                        SetHover(false);
                };
            }

            Wire(panel);
            foreach (Control child in panel.Controls)
                Wire(child);
        }

        private Panel CreateTitleBarButtons(Panel titleBar)
        {
            const int btnSize = 16;
            const int gap = 4;
            var panel = new Panel { Size = new Size(btnSize * 3 + gap * 2, btnSize), BackColor = Color.Transparent };

            for (var i = 0; i < 3; i++)
            {
                var index = i;
                var btn = new Panel
                {
                    Location = new Point(index * (btnSize + gap), 0),
                    Size = new Size(btnSize, btnSize),
                    BackColor = index == 2 ? Color.FromArgb(196, 43, 28) : AppTheme.Navy,
                    Cursor = index == 2 ? Cursors.Hand : Cursors.Default
                };
                btn.Paint += (s, e) =>
                {
                    using (var pen = new Pen(Color.FromArgb(74, 106, 144)))
                        e.Graphics.DrawRectangle(pen, 0, 0, btnSize - 1, btnSize - 1);
                    var symbol = index == 0 ? "─" : index == 1 ? "□" : "✕";
                    TextRenderer.DrawText(e.Graphics, symbol, new Font("Segoe UI", 6.5f),
                        new Rectangle(0, 0, btnSize, btnSize), Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                };
                if (index == 2)
                    btn.MouseClick += (s, e) => Close();
                panel.Controls.Add(btn);
            }

            void Align()
            {
                panel.Location = new Point(titleBar.Width - 12 - panel.Width, (titleBar.Height - panel.Height) / 2);
            }

            titleBar.Resize += (s, e) => Align();
            Align();
            return panel;
        }

        private Panel CreateStatusBar()
        {
            var bar = new Panel { Height = 22, BackColor = AppTheme.StatusBarBg };
            bar.Paint += (s, e) =>
            {
                using (var pen = new Pen(AppTheme.Border))
                    e.Graphics.DrawLine(pen, 0, 0, bar.Width, 0);
            };

            var dot = new Panel { Size = new Size(8, 8), BackColor = AppTheme.ConnectedGreen };
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, 8, 8);
                dot.Region = new Region(path);
            }

            var connected = AgroRegionApp.Data.AuthService.TestConnection(out var msg);
            if (!connected)
                dot.BackColor = AppTheme.Danger;

            var lblConn = new Label
            {
                Text = msg,
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent
            };
            var lblUser = new Label
            {
                Text = $"Пользователь: {_user.DisplayName}",
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent
            };
            var lblRole = new Label
            {
                Text = $"Роль: {_user.RoleName}",
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent
            };
            var lblVer = new Label
            {
                Text = $"{AppBranding.SystemTitle} v1.0",
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            bar.Controls.AddRange(new Control[] { dot, lblConn, lblUser, lblRole, lblVer });
            bar.Resize += (s, e) =>
            {
                var y = (bar.Height - 8) / 2;
                dot.Location = new Point(12, y);
                lblConn.Location = new Point(24, 3);
                lblUser.Location = new Point(lblConn.Right + 16, 3);
                lblRole.Location = new Point(lblUser.Right + 16, 3);
                lblVer.Location = new Point(bar.Width - lblVer.Width - 12, 3);
            };
            bar.PerformLayout();
            return bar;
        }

        private void ShowSection(NavSection section)
        {
            _currentSection = section;
            var item = RoleNavigation.GetItem(section);

            _lblTitleSection.Text = "—  " + item.Label;
            _lblSectionIcon.Text = item.Icon;
            _lblSectionTitle.Text = item.Label;

            foreach (var btn in _navButtons)
            {
                var state = (NavButtonState)btn.Tag;
                state.Active = state.Item.Section == section;
                ApplyNavButtonStyle(btn);
            }

            if (!_screens.TryGetValue(section, out var screen))
            {
                screen = CreateScreen(section);
                _screens[section] = screen;
            }

            _contentHost.Controls.Clear();
            screen.Dock = DockStyle.Fill;
            _contentHost.Controls.Add(screen);

            if (screen is INavigationScreen navigationScreen)
                navigationScreen.OnNavigatedTo();
        }

        private Control CreateScreen(NavSection section)
        {
            switch (section)
            {
                case NavSection.Sales: return new SalesView(_user);
                case NavSection.Warehouse: return new WarehouseView();
                case NavSection.Purchases: return new PurchasesView(_user);
                case NavSection.References: return new ReferencesView();
                case NavSection.Analytics: return new AnalyticsView();
                default: return new Panel();
            }
        }

        private void ConfirmLogout()
        {
            using (var form = new Form())
            {
                form.Text = "Выход из системы";
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.ClientSize = new Size(360, 160);
                form.Font = AppTheme.FontUi;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var header = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 26,
                    BackColor = AppTheme.Navy
                };
                header.Controls.Add(new Label
                {
                    Text = "Выход из системы",
                    Dock = DockStyle.Fill,
                    ForeColor = Color.White,
                    Font = AppTheme.FontUiBold,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0)
                });

                var body = new Label
                {
                    Text = "Вы действительно хотите завершить рабочую сессию?\r\nВсе открытые формы будут закрыты.",
                    Location = new Point(16, 40),
                    Size = new Size(328, 48),
                    Font = AppTheme.FontUi,
                    ForeColor = AppTheme.TextBody
                };

                var btnYes = UiControls.CreateButton("Да, выйти", true, 90);
                btnYes.Location = new Point(168, 108);
                btnYes.DialogResult = DialogResult.Yes;
                var btnNo = UiControls.CreateButton("Отмена", false, 80);
                btnNo.Location = new Point(264, 108);
                btnNo.DialogResult = DialogResult.No;

                form.Controls.Add(header);
                form.Controls.Add(body);
                form.Controls.Add(btnYes);
                form.Controls.Add(btnNo);

                if (form.ShowDialog(this) == DialogResult.Yes)
                {
                    ReturnToLogin = true;
                    Close();
                }
            }
        }
    }
}
