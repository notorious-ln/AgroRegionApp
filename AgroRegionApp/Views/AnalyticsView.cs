using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AgroRegionApp;
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
        private bool _loaded;
        private AnalyticsSummary _currentData;

        public AnalyticsView()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.ContentBg;
            Padding = new Padding(0, 4, 0, 0);

            _kpiPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = AppTheme.ContentBg };
            _kpiPanel.Resize += (s, e) => LayoutKpiCards();

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

            var btnPrint = UiControls.CreateButton("🖨  Печать отчёта", true, 130);
            btnPrint.Location = new Point(220, 4);
            btnPrint.Click += (s, e) => OnPrintReport();
            var btnExport = UiControls.CreateButton("📤  Экспорт в Excel", false, 140);
            btnExport.Location = new Point(356, 4);
            btnExport.Click += (s, e) => OnExportExcel();

            toolbar.Controls.Add(lblPeriod);
            toolbar.Controls.Add(_periodCombo);
            toolbar.Controls.Add(btnPrint);
            toolbar.Controls.Add(btnExport);

            var tabStrip = UiControls.CreateTabStrip(new[]
            {
                "📈  Продажи и закупки",
                "🏭  Складские запасы",
                "💳  Дебиторская задолженность"
            });
            tabStrip.TabChanged += index =>
            {
                _activeTab = index;
                if (_loaded) RefreshData();
            };

            _contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.ContentBg };

            Controls.Add(_contentPanel);
            Controls.Add(tabStrip);
            Controls.Add(toolbar);
            Controls.Add(_kpiPanel);

            _periodCombo.SelectedIndexChanged += (s, e) =>
            {
                if (_loaded) RefreshData();
            };

            Load += (s, e) =>
            {
                _loaded = true;
                RefreshData();
            };
        }

        private int SelectedYear => _periodCombo.SelectedIndex == 1 ? 2025 : 2026;

        private void RefreshData()
        {
            AnalyticsSummary data;
            try
            {
                data = AnalyticsService.Load(SelectedYear);
                _currentData = data;
            }
            catch (Exception ex)
            {
                ShowError("Не удалось загрузить данные: " + ex.Message);
                return;
            }

            try { BuildKpis(data); }
            catch { /* ignore KPI errors */ }

            try { BuildTabContent(data); }
            catch (Exception ex) { ShowError("Не удалось отобразить отчёт: " + ex.Message); }
        }

        private void ShowError(string message)
        {
            _contentPanel.Controls.Clear();
            _contentPanel.Controls.Add(new Label
            {
                Text = message,
                Dock = DockStyle.Top,
                AutoSize = true,
                MaximumSize = new Size(_contentPanel.Width > 0 ? _contentPanel.Width - 16 : 800, 0),
                ForeColor = AppTheme.Danger,
                Font = AppTheme.FontUi,
                Padding = new Padding(8)
            });
        }

        private AnalyticsSummary GetReportData()
        {
            if (_currentData != null)
                return _currentData;
            return AnalyticsService.Load(SelectedYear);
        }

        private void OnPrintReport()
        {
            try
            {
                var year = SelectedYear;
                var data = GetReportData();
                var folder = AnalyticsReportService.GetDefaultOutputFolder(year);

                using (var dlg = new FolderBrowserDialog
                {
                    Description = "Выберите папку для сохранения отчёта (Word и PDF)",
                    SelectedPath = folder
                })
                {
                    if (dlg.ShowDialog(FindForm()) != DialogResult.OK)
                        return;
                    folder = dlg.SelectedPath;
                }

                var previousCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                (string WordPath, string PdfPath) files;
                try
                {
                    files = AnalyticsReportService.GenerateWordAndPdf(data, year, folder);
                }
                finally
                {
                    Cursor.Current = previousCursor;
                }

                MessageBox.Show(
                    $"Отчёт сформирован:\n\n{files.WordPath}\n{files.PdfPath}",
                    AppBranding.SystemTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                Process.Start("explorer.exe", folder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnExportExcel()
        {
            try
            {
                var year = SelectedYear;
                var data = GetReportData();
                var defaultPath = Path.Combine(
                    AnalyticsReportService.GetDefaultOutputFolder(year),
                    $"Analitika_{year}.xls");

                using (var dlg = new SaveFileDialog
                {
                    Filter = "Excel (*.xls)|*.xls|Все файлы (*.*)|*.*",
                    FileName = Path.GetFileName(defaultPath),
                    InitialDirectory = Path.GetDirectoryName(defaultPath),
                    Title = "Экспорт аналитики в Excel"
                })
                {
                    if (dlg.ShowDialog(FindForm()) != DialogResult.OK)
                        return;

                    AnalyticsReportService.ExportToExcel(data, year, dlg.FileName);
                    MessageBox.Show(
                        "Данные экспортированы:\n" + dlg.FileName,
                        AppBranding.SystemTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    Process.Start(dlg.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BuildKpis(AnalyticsSummary data)
        {
            _kpiPanel.Controls.Clear();
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0)
            };

            flow.Controls.Add(UiControls.CreateKpiCard("📋", "Продажи за период",
                AnalyticsService.FormatMoneyShort(data.TotalSales), AppTheme.Blue));
            flow.Controls.Add(UiControls.CreateKpiCard("🛒", "Закупки за период",
                AnalyticsService.FormatMoneyShort(data.TotalPurchases), Color.FromArgb(124, 58, 237)));
            flow.Controls.Add(UiControls.CreateKpiCard("🏭", "Итого остатки",
                $"{data.TotalStockTons} т", AppTheme.ConnectedGreen));
            flow.Controls.Add(UiControls.CreateKpiCard("💳", "Дебиторка",
                AnalyticsService.FormatMoney(data.TotalDebt), AppTheme.Danger));

            _kpiPanel.Controls.Add(flow);
            LayoutKpiCards();
        }

        private void LayoutKpiCards()
        {
            if (_kpiPanel.Controls.Count == 0) return;
            var flow = _kpiPanel.Controls[0] as FlowLayoutPanel;
            if (flow == null) return;
            var cardWidth = Math.Max((_kpiPanel.Width - 24) / 4, 120);
            foreach (Control c in flow.Controls)
                c.Width = cardWidth;
        }

        private void BuildTabContent(AnalyticsSummary data)
        {
            _contentPanel.Controls.Clear();

            if (_activeTab == 0)
                ShowSalesPurchasesTab(data);
            else if (_activeTab == 1)
                ShowStocksTab(data);
            else
                ShowDebtsTab(data);
        }

        private void ShowSalesPurchasesTab(AnalyticsSummary data)
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = AppTheme.ContentBg,
                SplitterWidth = 6
            };

            split.Panel1.Controls.Add(BuildSalesPanel(data));
            split.Panel2.Controls.Add(BuildComparisonPanel(data));
            split.Panel1.Controls[0].Dock = DockStyle.Fill;
            split.Panel2.Controls[0].Dock = DockStyle.Fill;

            split.HandleCreated += (s, e) => ApplySplitDistance(split);
            split.Resize += (s, e) => ApplySplitDistance(split);

            _contentPanel.Controls.Add(split);
        }

        private static void ApplySplitDistance(SplitContainer split)
        {
            if (split.Width < 80) return;
            var half = split.Width / 2;
            var max = split.Width - split.Panel2MinSize - split.SplitterWidth;
            if (half >= split.Panel1MinSize && half <= max)
                split.SplitterDistance = half;
        }

        private Control BuildSalesPanel(AnalyticsSummary data)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.CardBg,
                Padding = new Padding(8, 4, 8, 8)
            };

            var chart = new AnalyticsChartControl
            {
                Dock = DockStyle.Top,
                Height = 180,
                ChartKind = AnalyticsChartKind.Bar
            };
            chart.Points = data.SalesByMonth.Select(m => new AnalyticsChartPoint
            {
                Label = m.MonthName,
                Value1 = m.Amount,
                Series1Name = "Продажи"
            }).ToList();

            var grid = UiControls.CreateGrid();
            grid.Dock = DockStyle.Fill;
            BindSalesGrid(grid, data);

            panel.Controls.Add(grid);
            panel.Controls.Add(chart);
            return WrapSection("Сводка продаж по месяцам (сумма, ₽)", panel);
        }

        private Control BuildComparisonPanel(AnalyticsSummary data)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.CardBg,
                Padding = new Padding(8, 4, 8, 8)
            };

            var chart = new AnalyticsChartControl
            {
                Dock = DockStyle.Top,
                Height = 180,
                ChartKind = AnalyticsChartKind.Line
            };
            chart.Points = data.SalesByMonth.Select((s, i) =>
            {
                var p = i < data.PurchasesByMonth.Count ? data.PurchasesByMonth[i] : new MonthlyAmountRow();
                return new AnalyticsChartPoint
                {
                    Label = s.MonthName,
                    Value1 = s.Amount,
                    Value2 = p.Amount,
                    Series1Name = "Продажи",
                    Series2Name = "Закупки"
                };
            }).ToList();

            var grid = UiControls.CreateGrid();
            grid.Dock = DockStyle.Fill;
            BindComparisonGrid(grid, data);

            panel.Controls.Add(grid);
            panel.Controls.Add(chart);
            return WrapSection("Сравнение: продажи vs закупки (₽)", panel);
        }

        private static Control WrapSection(string title, Control content)
        {
            var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.CardBg, Padding = new Padding(1) };
            wrapper.Paint += (s, e) =>
            {
                using (var pen = new Pen(AppTheme.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, wrapper.Width - 1, wrapper.Height - 1);
            };

            var lbl = new Label
            {
                Text = title.ToUpperInvariant(),
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = AppTheme.TextMuted,
                BackColor = AppTheme.CardBg,
                Padding = new Padding(8, 6, 0, 0)
            };

            content.Dock = DockStyle.Fill;
            wrapper.Controls.Add(content);
            wrapper.Controls.Add(lbl);
            return wrapper;
        }

        private static void BindSalesGrid(DataGridView grid, AnalyticsSummary data)
        {
            grid.DataSource = data.SalesByMonth.Select(m => new
            {
                Month = m.MonthName,
                QtyTons = m.QuantityTons,
                Amount = AnalyticsFormat.Money(m.Amount)
            }).ToList();

            grid.DataBindingComplete += (s, e) =>
            {
                if (grid.Columns.Contains("Month")) grid.Columns["Month"].HeaderText = "Месяц";
                if (grid.Columns.Contains("QtyTons")) grid.Columns["QtyTons"].HeaderText = "Кол-во (т)";
                if (grid.Columns.Contains("Amount")) grid.Columns["Amount"].HeaderText = "Сумма (₽)";
            };
        }

        private static void BindComparisonGrid(DataGridView grid, AnalyticsSummary data)
        {
            grid.DataSource = data.SalesByMonth.Select((s, i) =>
            {
                var p = i < data.PurchasesByMonth.Count ? data.PurchasesByMonth[i] : new MonthlyAmountRow();
                return new
                {
                    Month = s.MonthName,
                    Sales = AnalyticsFormat.Money(s.Amount),
                    Purchases = AnalyticsFormat.Money(p.Amount),
                    Diff = AnalyticsFormat.Diff(s.Amount - p.Amount)
                };
            }).ToList();

            grid.DataBindingComplete += (s, e) =>
            {
                if (grid.Columns.Contains("Month")) grid.Columns["Month"].HeaderText = "Месяц";
                if (grid.Columns.Contains("Sales")) grid.Columns["Sales"].HeaderText = "Продажи (₽)";
                if (grid.Columns.Contains("Purchases")) grid.Columns["Purchases"].HeaderText = "Закупки (₽)";
                if (grid.Columns.Contains("Diff")) grid.Columns["Diff"].HeaderText = "Разница (₽)";
            };
        }

        private void ShowStocksTab(AnalyticsSummary data)
        {
            var box = UiControls.CreateGroupBox("Текущие складские запасы (по последним ручным данным)");
            box.Dock = DockStyle.Fill;

            var warning = UiControls.CreateInfoBar(
                "⚠  Данные об остатках вносятся вручную после сверки с бумажными журналами. Фактические значения могут отличаться.",
                AppTheme.WarnBg, AppTheme.WarnBorder, Color.FromArgb(146, 64, 14));

            var grid = UiControls.CreateGrid();
            grid.Dock = DockStyle.Top;
            grid.Height = 130;
            grid.DataSource = data.Stocks.Select(s => new
            {
                Product = s.ProductName,
                Wh1 = s.Warehouse1Tons,
                Wh2 = s.Warehouse2Tons,
                Total = s.TotalTons,
                Checked = s.CheckDate?.ToString("dd.MM.yyyy") ?? "—",
                Status = s.Status
            }).ToList();
            grid.DataBindingComplete += (s, e) =>
            {
                SetHeader(grid, "Product", "Культура");
                SetHeader(grid, "Wh1", "Склад №1 (т)");
                SetHeader(grid, "Wh2", "Склад №2 (т)");
                SetHeader(grid, "Total", "Итого (т)");
                SetHeader(grid, "Checked", "Дата проверки");
            };
            GridHelper.ApplyStatusColumnFormatting(grid, "Status");

            var chart = new AnalyticsChartControl
            {
                Dock = DockStyle.Fill,
                ChartKind = AnalyticsChartKind.GroupedBar,
                UseThousandsSuffix = false,
                IsCurrency = false,
                Series1Color = AppTheme.Blue,
                Series2Color = AppTheme.ConnectedGreen
            };
            chart.Points = data.Stocks.Select(s => new AnalyticsChartPoint
            {
                Label = s.ProductName,
                Value1 = s.Warehouse1Tons,
                Value2 = s.Warehouse2Tons,
                Series1Name = "Склад №1",
                Series2Name = "Склад №2"
            }).ToList();

            var inner = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 0) };
            inner.Controls.Add(chart);
            inner.Controls.Add(grid);
            inner.Controls.Add(warning);

            box.Controls.Add(inner);
            _contentPanel.Controls.Add(box);
        }

        private void ShowDebtsTab(AnalyticsSummary data)
        {
            var box = UiControls.CreateGroupBox("Контроль дебиторской задолженности покупателей");
            box.Dock = DockStyle.Fill;

            var inner = new Panel { Dock = DockStyle.Fill };

            if (data.TotalDebt > 0)
            {
                var alert = UiControls.CreateInfoBar(
                    $"⚠  Общая дебиторская задолженность: {AnalyticsService.FormatMoney(data.TotalDebt)}",
                    AppTheme.WarnBg, AppTheme.WarnBorder, Color.FromArgb(146, 64, 14));
                inner.Controls.Add(BuildDebtGrid(data));
                inner.Controls.Add(alert);
            }
            else
            {
                inner.Controls.Add(BuildDebtGrid(data));
            }

            box.Controls.Add(inner);
            _contentPanel.Controls.Add(box);
        }

        private Control BuildDebtGrid(AnalyticsSummary data)
        {
            var grid = UiControls.CreateGrid();
            grid.Dock = DockStyle.Fill;
            grid.DataSource = data.Debtors.Select(d => new
            {
                Buyer = d.CustomerName,
                Orders = d.OrderCount,
                Total = AnalyticsFormat.Money(d.OrderTotal),
                Paid = AnalyticsFormat.Money(d.PaidAmount),
                Debt = d.DebtAmount > 0 ? AnalyticsFormat.Money(d.DebtAmount) : "—",
                Status = d.Status
            }).ToList();
            grid.DataBindingComplete += (s, e) =>
            {
                SetHeader(grid, "Buyer", "Покупатель");
                SetHeader(grid, "Orders", "Заказов");
                SetHeader(grid, "Total", "Сумма заказов (₽)");
                SetHeader(grid, "Paid", "Оплачено (₽)");
                SetHeader(grid, "Debt", "Задолженность (₽)");
            };
            GridHelper.ApplyStatusColumnFormatting(grid, "Status");
            return grid;
        }

        private static void SetHeader(DataGridView grid, string column, string header)
        {
            if (grid.Columns.Contains(column))
                grid.Columns[column].HeaderText = header;
        }
    }
}
