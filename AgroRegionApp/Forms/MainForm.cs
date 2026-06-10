using System;
using System.Drawing;
using System.Windows.Forms;
using AgroRegionApp.Models;

namespace AgroRegionApp.Forms
{
    public partial class MainForm : Form
    {
        private static readonly Color Navy = Color.FromArgb(30, 53, 88);

        private readonly AuthenticatedUser _user;

        public bool ReturnToLogin { get; private set; }

        public MainForm(AuthenticatedUser user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            InitializeComponent();
            BuildLayout();
        }

        private void BuildLayout()
        {
            var titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 28,
                BackColor = Navy
            };

            titleBar.Controls.Add(new Label
            {
                Text = $"🌾  {AppBranding.SystemTitle} — Главное меню",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            });

            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(24)
            };

            content.Controls.Add(new Label
            {
                Text = $"Добро пожаловать, {_user.DisplayName}",
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Navy
            });

            content.Controls.Add(new Label
            {
                Text = $"Роль: {_user.RoleName}",
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(107, 114, 128)
            });

            content.Controls.Add(new Label
            {
                Text = "Разделы приложения будут добавлены на следующих этапах разработки.",
                Dock = DockStyle.Top,
                Height = 48,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(55, 65, 81)
            });

            var btnLogout = new Button
            {
                Text = "Выйти из профиля",
                Size = new Size(140, 28),
                Location = new Point(24, 120),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.25f),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderColor = Color.FromArgb(192, 199, 208);
            btnLogout.Click += (s, e) =>
            {
                ReturnToLogin = true;
                Close();
            };
            content.Controls.Add(btnLogout);

            Controls.Add(content);
            Controls.Add(titleBar);
        }
    }
}
