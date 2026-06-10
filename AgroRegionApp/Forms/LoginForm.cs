using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AgroRegionApp.Data;
using AgroRegionApp.Models;

namespace AgroRegionApp.Forms
{
    public partial class LoginForm : Form
    {
        private static readonly Color Navy = Color.FromArgb(30, 53, 88);
        private static readonly Color Blue = Color.FromArgb(46, 117, 182);
        private static readonly Color CardBg = Color.FromArgb(245, 246, 248);
        private static readonly Color BorderGray = Color.FromArgb(156, 163, 175);
        private static readonly Color FieldBorder = Color.FromArgb(192, 199, 208);
        private static readonly Color TextDark = Color.FromArgb(55, 65, 81);
        private static readonly Color TextMuted = Color.FromArgb(107, 114, 128);
        private static readonly Color TextLight = Color.FromArgb(156, 163, 175);
        private static readonly Color StatusBarBg = Color.FromArgb(229, 231, 235);
        private static readonly Color HintBg = Color.FromArgb(239, 246, 255);
        private static readonly Color HintBorder = Color.FromArgb(191, 219, 254);
        private static readonly Color HintText = Color.FromArgb(59, 130, 246);
        private static readonly Color ErrorBg = Color.FromArgb(254, 242, 242);
        private static readonly Color ErrorBorder = Color.FromArgb(254, 202, 202);
        private static readonly Color ErrorText = Color.FromArgb(220, 38, 38);
        private static readonly Color ConnectedGreen = Color.FromArgb(22, 163, 74);

        private Panel _card;
        private TextBox _txtLogin;
        private TextBox _txtPassword;
        private Panel _errorPanel;
        private Label _lblError;
        private Label _lblConnection;
        private Panel _connectionDot;
        private Label _lblDate;
        private Panel _statusBar;

        public AuthenticatedUser AuthenticatedUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            BuildLayout();
            UpdateConnectionStatus();
        }

        private void BuildLayout()
        {
            SuspendLayout();

            _card = new Panel
            {
                Size = new Size(360, 420),
                BackColor = CardBg
            };
            _card.Paint += Card_Paint;
            Controls.Add(_card);

            var titleBar = CreateTitleBar();
            _card.Controls.Add(titleBar);

            var body = new Panel
            {
                Location = new Point(0, 28),
                Size = new Size(360, 370),
                BackColor = CardBg
            };
            _card.Controls.Add(body);

            var logoPanel = CreateLogoSection();
            logoPanel.Location = new Point(24, 8);
            body.Controls.Add(logoPanel);

            var fieldsPanel = CreateFieldsSection();
            fieldsPanel.Location = new Point(24, 108);
            body.Controls.Add(fieldsPanel);

            _errorPanel = CreateErrorPanel();
            _errorPanel.Location = new Point(24, 178);
            _errorPanel.Visible = false;
            body.Controls.Add(_errorPanel);

            var buttonsPanel = CreateButtonsPanel();
            buttonsPanel.Location = new Point(24, 210);
            body.Controls.Add(buttonsPanel);

            var hintPanel = CreateHintPanel();
            hintPanel.Location = new Point(24, 252);
            body.Controls.Add(hintPanel);

            var adminHint = new Label
            {
                Text = "Для получения учётных данных обратитесь к администратору системы",
                Location = new Point(24, 310),
                Size = new Size(312, 32),
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = TextLight,
                TextAlign = ContentAlignment.TopCenter
            };
            body.Controls.Add(adminHint);

            _statusBar = CreateStatusBar();
            _card.Controls.Add(_statusBar);
            UpdateCardLayout();

            ResumeLayout(false);
            CenterCard();
        }

        private Panel CreateTitleBar()
        {
            var bar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(360, 28),
                BackColor = Navy
            };

            var icon = new Label
            {
                Text = "🌾",
                Location = new Point(8, 0),
                Size = new Size(20, 28),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var title = new Label
            {
                Text = $"{AppBranding.SystemTitle} — Вход в систему",
                Location = new Point(30, 0),
                Size = new Size(250, 28),
                Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            bar.Controls.Add(icon);
            bar.Controls.Add(title);
            bar.Controls.Add(CreateWindowButtons(bar));
            return bar;
        }

        private Panel CreateWindowButtons(Panel titleBar)
        {
            const int btnSize = 16;
            const int gap = 4;
            var symbols = new[] { "─", "□", "✕" };
            var borderColor = Color.FromArgb(74, 106, 144);
            var closeColor = Color.FromArgb(196, 43, 28);
            var symbolFont = new Font("Segoe UI", 6.5f);

            var panel = new Panel
            {
                Size = new Size(btnSize * 3 + gap * 2, btnSize),
                BackColor = Color.Transparent
            };

            for (var i = 0; i < symbols.Length; i++)
            {
                var index = i;
                var btn = new Panel
                {
                    Location = new Point(index * (btnSize + gap), 0),
                    Size = new Size(btnSize, btnSize),
                    BackColor = index == 2 ? closeColor : Navy,
                    Cursor = index == 2 ? Cursors.Hand : Cursors.Default
                };

                btn.Paint += (s, e) =>
                {
                    using (var pen = new Pen(borderColor))
                        e.Graphics.DrawRectangle(pen, 0, 0, btnSize - 1, btnSize - 1);

                    TextRenderer.DrawText(
                        e.Graphics,
                        symbols[index],
                        symbolFont,
                        new Rectangle(0, 0, btnSize, btnSize),
                        Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                };

                if (index == 2)
                    btn.MouseClick += (s, e) => Close();

                panel.Controls.Add(btn);
            }

            void AlignButtons()
            {
                panel.Location = new Point(
                    titleBar.Width - 12 - panel.Width,
                    (titleBar.Height - panel.Height) / 2);
            }

            titleBar.Resize += (s, e) => AlignButtons();
            AlignButtons();
            return panel;
        }

        private Panel CreateLogoSection()
        {
            var panel = new Panel
            {
                Size = new Size(312, 88),
                BackColor = CardBg
            };

            panel.Controls.Add(new Label
            {
                Text = AppBranding.SystemTitle,
                Location = new Point(0, 0),
                Size = new Size(312, 28),
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Navy,
                TextAlign = ContentAlignment.MiddleCenter
            });

            panel.Controls.Add(new Label
            {
                Text = "Система управления коммерческой деятельностью",
                Location = new Point(0, 30),
                Size = new Size(312, 20),
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = TextMuted,
                TextAlign = ContentAlignment.MiddleCenter
            });

            panel.Controls.Add(new Label
            {
                Text = AppBranding.CompanyName,
                Location = new Point(0, 52),
                Size = new Size(312, 18),
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = TextLight,
                TextAlign = ContentAlignment.MiddleCenter
            });

            panel.Controls.Add(new Panel
            {
                Location = new Point(0, 84),
                Size = new Size(312, 1),
                BackColor = Color.FromArgb(229, 231, 235)
            });

            return panel;
        }

        private Panel CreateFieldsSection()
        {
            var panel = new Panel
            {
                Size = new Size(312, 62),
                BackColor = CardBg
            };

            var lblLogin = new Label
            {
                Text = "Логин:",
                Location = new Point(0, 4),
                Size = new Size(60, 22),
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = TextDark,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _txtLogin = CreateInput();
            _txtLogin.Location = new Point(72, 2);

            var lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new Point(0, 34),
                Size = new Size(60, 22),
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = TextDark,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _txtPassword = CreateInput();
            _txtPassword.Location = new Point(72, 32);
            _txtPassword.UseSystemPasswordChar = true;
            _txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    TryLogin();
                }
            };

            panel.Controls.Add(lblLogin);
            panel.Controls.Add(_txtLogin);
            panel.Controls.Add(lblPassword);
            panel.Controls.Add(_txtPassword);
            return panel;
        }

        private TextBox CreateInput()
        {
            return new TextBox
            {
                Size = new Size(240, 26),
                Font = new Font("Segoe UI", 8.25f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
        }

        private Panel CreateErrorPanel()
        {
            var panel = new Panel
            {
                Size = new Size(312, 28),
                BackColor = ErrorBg
            };
            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(ErrorBorder))
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            _lblError = new Label
            {
                Location = new Point(8, 5),
                Size = new Size(296, 18),
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = ErrorText,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(_lblError);
            return panel;
        }

        private Panel CreateButtonsPanel()
        {
            var panel = new Panel
            {
                Size = new Size(312, 30),
                BackColor = CardBg
            };

            var btnLogin = CreateButton("→  Войти", true);
            btnLogin.Location = new Point(78, 0);
            btnLogin.Click += (s, e) => TryLogin();

            var btnClear = CreateButton("Очистить", false);
            btnClear.Location = new Point(168, 0);
            btnClear.Click += (s, e) => ClearForm();

            panel.Controls.Add(btnLogin);
            panel.Controls.Add(btnClear);
            return panel;
        }

        private Button CreateButton(string text, bool primary)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(88, 26),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.25f),
                Cursor = Cursors.Hand
            };

            if (primary)
            {
                btn.BackColor = Blue;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderColor = Color.FromArgb(26, 90, 154);
            }
            else
            {
                btn.BackColor = Color.White;
                btn.ForeColor = Color.FromArgb(26, 26, 46);
                btn.FlatAppearance.BorderColor = FieldBorder;
            }

            btn.FlatAppearance.BorderSize = 1;
            return btn;
        }

        private Panel CreateHintPanel()
        {
            var panel = new Panel
            {
                Size = new Size(312, 48),
                BackColor = HintBg
            };
            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(HintBorder))
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            panel.Controls.Add(new Label
            {
                Text = "Введите логин и пароль учётной записи AgroCompany",
                Location = new Point(8, 6),
                Size = new Size(296, 36),
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = HintText,
                TextAlign = ContentAlignment.MiddleCenter
            });

            return panel;
        }

        private Panel CreateStatusBar()
        {
            const int barHeight = 22;

            var bar = new Panel
            {
                Size = new Size(360, barHeight),
                BackColor = StatusBarBg
            };
            bar.Paint += (s, e) =>
            {
                using (var pen = new Pen(FieldBorder))
                    e.Graphics.DrawLine(pen, 0, 0, bar.Width, 0);
            };

            _connectionDot = new Panel
            {
                Size = new Size(8, 8),
                BackColor = ConnectedGreen
            };
            MakeCircle(_connectionDot);

            var statusFont = new Font("Segoe UI", 8.25f);
            _lblConnection = new Label
            {
                AutoSize = true,
                Font = statusFont,
                ForeColor = TextMuted,
                BackColor = Color.Transparent
            };

            _lblDate = new Label
            {
                AutoSize = true,
                Font = statusFont,
                ForeColor = TextMuted,
                BackColor = Color.Transparent,
                Text = DateTime.Now.ToString("dd.MM.yyyy")
            };

            bar.Controls.Add(_connectionDot);
            bar.Controls.Add(_lblConnection);
            bar.Controls.Add(_lblDate);
            bar.Resize += (s, e) => LayoutStatusBar(bar);
            LayoutStatusBar(bar);
            return bar;
        }

        private void LayoutStatusBar(Panel bar)
        {
            const int barHeight = 22;
            const int dotSize = 8;
            const int leftPadding = 12;

            var dotY = (barHeight - dotSize) / 2;
            _connectionDot.Location = new Point(leftPadding, dotY);

            var textHeight = TextRenderer.MeasureText("Ag", _lblConnection.Font).Height;
            var textY = (barHeight - textHeight) / 2;
            _lblConnection.Location = new Point(leftPadding + dotSize + 4, textY);
            _lblDate.Location = new Point(bar.Width - _lblDate.Width - leftPadding, textY);
        }

        private void UpdateCardLayout()
        {
            if (_statusBar == null)
                return;

            _statusBar.Location = new Point(0, _card.Height - _statusBar.Height);
            LayoutStatusBar(_statusBar);
        }

        private static void MakeCircle(Panel panel)
        {
            var path = new GraphicsPath();
            path.AddEllipse(0, 0, panel.Width, panel.Height);
            panel.Region = new Region(path);
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(BorderGray))
                e.Graphics.DrawRectangle(pen, 0, 0, _card.Width - 1, _card.Height - 1);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var rect = ClientRectangle;
            using (var brush = new LinearGradientBrush(rect, Navy, Blue, 135f))
                e.Graphics.FillRectangle(brush, rect);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterCard();
        }

        private void CenterCard()
        {
            if (_card == null)
                return;

            _card.Location = new Point(
                Math.Max(0, (ClientSize.Width - _card.Width) / 2),
                Math.Max(0, (ClientSize.Height - _card.Height) / 2));
        }

        private void UpdateConnectionStatus()
        {
            var connected = AuthService.TestConnection(out var message);
            _lblConnection.Text = message;
            _connectionDot.BackColor = connected ? ConnectedGreen : Color.FromArgb(220, 38, 38);
            if (_statusBar != null)
                LayoutStatusBar(_statusBar);
        }

        private void ShowError(string message)
        {
            _lblError.Text = message;
            _errorPanel.Visible = true;
            _card.Height = 448;
            UpdateCardLayout();
            CenterCard();
        }

        private void HideError()
        {
            _errorPanel.Visible = false;
            _card.Height = 420;
            UpdateCardLayout();
            CenterCard();
        }

        private void ClearForm()
        {
            _txtLogin.Clear();
            _txtPassword.Clear();
            HideError();
            _txtLogin.Focus();
        }

        private void TryLogin()
        {
            HideError();

            var result = AuthService.Authenticate(_txtLogin.Text, _txtPassword.Text);
            if (result.Success)
            {
                AuthenticatedUser = result.User;
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            switch (result.FailureReason)
            {
                case AuthFailureReason.Blocked:
                    ShowError("Доступ ограничен. Данный аккаунт заблокирован.");
                    break;
                case AuthFailureReason.DatabaseError:
                    ShowError("Ошибка подключения к базе данных. Проверьте SQL Server.");
                    UpdateConnectionStatus();
                    break;
                default:
                    ShowError("Ошибка авторизации. Неверный логин или пароль.");
                    _txtPassword.Clear();
                    _txtPassword.Focus();
                    break;
            }
        }
    }
}
