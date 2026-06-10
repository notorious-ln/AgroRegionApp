using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AgroRegionApp.Data;
using AgroRegionApp.UI;

namespace AgroRegionApp.Views
{
    internal sealed class AnalyticsView : UserControl
    {
        private readonly Panel _kpiPanel;
        private readonly Panel _contentPanel;
        private readonly ComboBox _periodCombo;
        private int _activeTab;

        public AnalyticsView()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.ContentBg;
            Padding = new Padding(0, 4, 0, 0);

            _kpiPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = AppTheme.ContentBg };
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = AppTheme.ContentBg };

            var lblPeriod = UiControls.CreateFieldLabel("Период:", 60);
            lblPeriod.Location = new Point(0, 6);
            _periodCombo = new ComboBox
            {
                Location = new Point(64, 4),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTheme.FontUi
            };
            _periodCombo.Items.AddRange(new object[] { "2026 год", "2025 год" });
            _periodCombo.SelectedIndex = 0;
            _periodCombo.SelectedIndexChanged += (s, e) => RefreshData();

            var btnPrint = UiControls.CreateButton("Печать отчёта", true, 120);
            btnPrint.Location = new Point(220, 4);
            toolbar.Controls.Add(lblPeriod);
            toolbar.Controls.Add(_periodCombo);
            toolbar.Controls.Add(btnPrint);

            var tabStrip = UiControls.CreateTabStrip(
                new[] { "Продажи и закупки", "Складские запасы", "Дебиторская задолженность" });
            tabStrip.TabChanged += index => { _activeTab = index; RefreshData(); };

            _contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.ContentBg };

            Controls.Add(_contentPanel);
            Controls.Add(tabStrip);
            Controls.Add(toolbar);
            Controls.Add(_kpiPanel);

            RefreshData();
        }

        private int SelectedYear => _periodCombo.SelectedIndex == 1 ? 2025 : 2026;

        private void RefreshData()
        {
            try
            {
                var data = AnalyticsService.Load(SelectedYear);
                BuildKpis(data);
                BuildTabContent(data);
            }
            catch (Exception ex)
            {
                _contentPanel.Controls.Clear();
                _contentPanel.Controls.Add(new Label
                {
                    Text = "Не удалось загрузить аналитику: " + ex.Message,
                    Dock = DockStyle.Top,
                    ForeColor = AppTheme.Danger,
                    Font = AppTheme.FontUi,
                    Padding = new Padding(8)
                });
            }
        }

        private void BuildKpis(AnalyticsSummary data)
        {
            _kpiPanel.Controls.Clear();
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            flow.Controls.Add(UiControls.CreateKpiCard("📋", "Продажи за период",
                $"{data.TotalSales / 1_000_000m:0.00} млн ₽", AppTheme.Blue));
            flow.Controls.Add(UiControls.CreateKpiCard("🛒", "Закупки за период",
                $"{data.TotalPurchases / 1_000_000m:0.00} млн ₽", Color.FromArgb(124, 58, 237)));
            flow.Controls.Add(UiControls.CreateKpiCard("🏭", "Итого остатки",
                $"{data.TotalStockTons} т", AppTheme.ConnectedGreen));
            flow.Controls.Add(UiControls.CreateKpiCard("💳", "Заказов продаж",
                data.OrderCount.ToString(), AppTheme.Danger));

            foreach (Control c in flow.Controls)
                c.Width = (_kpiPanel.Width - 32) / 4;

            _kpiPanel.Controls.Add(flow);
        }

        private void BuildTabContent(AnalyticsSummary data)
        {
            _contentPanel.Controls.Clear();

            if (_activeTab == 0)
                ShowSalesPurchasesTab(data);
            else if (_activeTab == 1)
                ShowStocksTab(data);
            else
                ShowDebtsTab();
        }

        private void ShowSalesPurchasesTab(AnalyticsSummary data)
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = AppTheme.ContentBg
            };

            split.Panel1.Controls.Add(CreateGridBox("Сводка продаж по месяцам",
                data.SalesByMonth.Select(m => new
                {
                    Месяц = m.MonthName,
                    Заказов = m.Count,
                    Сумма = m.Amount.ToString("N0") + " ₽"
                }).ToList()));

            var purchaseRows = data.PurchasesByMonth.Select(m => new
            {
                Месяц = m.MonthName,
                Заказов = m.Count,
                Сумма = m.Amount.ToString("N0") + " ₽"
            }).ToList();

            split.Panel2.Controls.Add(CreateGridBox("Закупки по месяцам", purchaseRows));
            _contentPanel.Controls.Add(split);
        }

        private void ShowStocksTab(AnalyticsSummary data)
        {
            _contentPanel.Controls.Add(UiControls.CreateInfoBar(
                "Данные об остатках вносятся вручную после сверки с бумажными журналами.",
                AppTheme.WarnBg, AppTheme.WarnBorder));

            var box = CreateGridBox("Текущие складские запасы", data.Stocks.Select(s => new
            {
                Культура = s.ProductName,
                Склад = s.WarehouseName,
                Количество = s.Quantity + " т",
                Проверка = s.CheckDate?.ToString("dd.MM.yyyy") ?? "—",
                Состояние = s.Quantity == 0 ? "Нет в наличии" : s.Quantity < 30 ? "Мало" : "В наличии"
            }).ToList());
            box.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(box);
        }

        private void ShowDebtsTab()
        {
            _contentPanel.Controls.Add(UiControls.CreateInfoBar(
                "Модуль дебиторской задолженности будет подключён после добавления платежей в БД.",
                AppTheme.HintBg, AppTheme.HintBorder));

            var grid = UiControls.CreateGrid();
            grid.Dock = DockStyle.Fill;
            grid.DataSource = new[]
            {
                new { Покупатель = "—", Заказов = 0, Сумма = "0 ₽", Оплачено = "0 ₽", Задолженность = "—", Статус = "Нет данных" }
            };

            var box = UiControls.CreateGroupBox("Контроль дебиторской задолженности");
            box.Dock = DockStyle.Fill;
            box.Controls.Add(grid);
            _contentPanel.Controls.Add(box);
        }

        private static GroupBox CreateGridBox(string title, object data)
        {
            var box = UiControls.CreateGroupBox(title);
            box.Dock = DockStyle.Fill;
            var grid = UiControls.CreateGrid();
            grid.Dock = DockStyle.Fill;
            grid.DataSource = data;
            box.Controls.Add(grid);
            return box;
        }
    }
}
